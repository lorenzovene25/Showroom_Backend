using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════════
//  ORDER SERVICE
// ══════════════════════════════════════════════════════════════════
public class OrderService : IOrderService
{
    private readonly string _connectionString;

    public OrderService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    private async Task<OrderDto?> HydrateAsync(
        NpgsqlConnection conn, int orderId, string culture)
    {
        // Aggiunti gli alias in PascalCase per il mapping sicuro con la Tupla
        var order = await conn.QuerySingleOrDefaultAsync<(int Id, int UserId, string Status, decimal TotalAmount, DateTime CreatedAt)>(
            "SELECT id AS Id, user_id AS UserId, status AS Status, total_amount AS TotalAmount, created_at AS CreatedAt FROM orders WHERE id = @Id",
            new { Id = orderId });

        if (order == default) return null;

        var items = await conn.QueryAsync<OrderItemDto>("""
            SELECT
                oi.id                     AS Id, 
                oi.order_id               AS OrderId, 
                oi.souvenir_id            AS SouvenirId,
                COALESCE(t.name, en.name) AS SouvenirName,
                oi.quantity               AS Quantity, 
                oi.unit_price             AS UnitPrice
            FROM order_items oi
            JOIN souvenirs s ON s.id = oi.souvenir_id
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE oi.order_id = @OrderId
            ORDER BY oi.id;
            """, new { OrderId = orderId, Culture = culture });

        return new OrderDto(){
            Id = order.Id, 
            UserId = order.UserId, 
            Status = order.Status,
            TotalAmount = order.TotalAmount, 
            CreatedAt = order.CreatedAt, 
            Items = items 
        };
    }

    // GET ALL (admin)
    public async Task<IEnumerable<OrderDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        var orderIds = await conn.QueryAsync<int>(
            "SELECT id FROM orders ORDER BY created_at DESC");

        var tasks = orderIds.Select(id => HydrateAsync(conn, id, culture));
        return (await Task.WhenAll(tasks)).Where(o => o is not null)!;
    }

    // GET BY USER
    public async Task<IEnumerable<OrderDto>> GetByUserAsync(int userId, string culture = "en")
    {
        using var conn = Conn();
        var orderIds = await conn.QueryAsync<int>(
            "SELECT id FROM orders WHERE user_id = @UserId ORDER BY created_at DESC",
            new { UserId = userId });

        var tasks = orderIds.Select(id => HydrateAsync(conn, id, culture));
        return (await Task.WhenAll(tasks)).Where(o => o is not null)!;
    }

    // GET BY ID
    public async Task<OrderDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await HydrateAsync(conn, id, culture);
    }

    // POST — creates order + items in one transaction; total is computed server-side
    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // fetch current prices to avoid trusting the client
            // Aggiunti gli alias per la tupla
            var souvenirIds = dto.Items.Select(i => i.SouvenirId).ToArray();
            var prices = (await conn.QueryAsync<(int Id, decimal Price)>(
                "SELECT id AS Id, price AS Price FROM souvenirs WHERE id = ANY(@Ids)",
                new { Ids = souvenirIds }, tx))
                .ToDictionary(x => x.Id, x => x.Price);

            decimal total = dto.Items.Sum(i =>
                prices.TryGetValue(i.SouvenirId, out var p) ? p * i.Quantity : 0m);

            int orderId = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO orders (user_id, status, total_amount)
                VALUES (@UserId, 'pending', @TotalAmount)
                RETURNING id AS Id;
                """, new { dto.UserId, TotalAmount = total }, tx);

            foreach (var item in dto.Items)
            {
                decimal unitPrice = prices.GetValueOrDefault(item.SouvenirId);
                await conn.ExecuteAsync("""
                    INSERT INTO order_items (order_id, souvenir_id, quantity, unit_price)
                    VALUES (@OrderId, @SouvenirId, @Quantity, @UnitPrice);
                    """,
                    new { OrderId = orderId, item.SouvenirId, item.Quantity, UnitPrice = unitPrice }, tx);
            }

            await tx.CommitAsync();
            return (await GetByIdAsync(orderId, culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PATCH — update status only
    public async Task<OrderDto?> PatchStatusAsync(int id, PatchOrderStatusDto dto, string culture = "en")
    {
        using var conn = Conn();
        int rows = await conn.ExecuteAsync(
            "UPDATE orders SET status = @Status WHERE id = @Id",
            new { Id = id, dto.Status });
        return rows == 0 ? null : await GetByIdAsync(id, culture);
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM orders WHERE id = @Id", new { Id = id }) > 0;
    }
}
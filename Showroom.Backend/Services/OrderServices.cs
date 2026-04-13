using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public class OrderService : IOrderService
{
    private readonly string _connectionString;

    public OrderService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    private async Task<OrderDto?> HydrateAsync(NpgsqlConnection conn, int orderId, string culture)
    {
        const string orderQuery = """
            SELECT 
                id           AS Id, 
                user_id      AS UserId, 
                status       AS Status, 
                total_amount AS TotalAmount, 
                created_at   AS CreatedAt 
            FROM orders 
            WHERE id = @Id
            """;
        var orderData = await conn.QuerySingleOrDefaultAsync<(int Id, int UserId, string Status, decimal TotalAmount, DateTime CreatedAt)>(orderQuery, new { Id = orderId });

        if (orderData == default) return null;

        const string itemsQuery = """
            SELECT
                oi.id                     AS Id, 
                oi.order_id               AS OrderId, 
                oi.souvenir_id            AS SouvenirId,
                COALESCE(t.name, en.name) AS SouvenirName,
                oi.unit_price             AS UnitPrice,
                oi.quantity               AS Quantity
            FROM order_items oi
            JOIN souvenirs s ON s.id = oi.souvenir_id
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE oi.order_id = @OrderId
            """;
        var items = await conn.QueryAsync<OrderItemDto>(itemsQuery, new { OrderId = orderId, Culture = culture });

        return new OrderDto
        {
            Id = orderData.Id,
            UserId = orderData.UserId,
            Status = orderData.Status,
            TotalAmount = orderData.TotalAmount,
            CreatedAt = orderData.CreatedAt,
            Items = items.ToList()
        };
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT id 
            FROM orders 
            ORDER BY created_at DESC
            """;
        var orderIds = await conn.QueryAsync<int>(query);

        var results = new List<OrderDto>();
        foreach (var id in orderIds)
        {
            var order = await HydrateAsync(conn, id, culture);
            if (order != null) results.Add(order);
        }
        return results;
    }

    public async Task<IEnumerable<OrderDto>> GetByUserAsync(int userId, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT id 
            FROM orders 
            WHERE user_id = @UserId 
            ORDER BY created_at DESC
            """;
        var orderIds = await conn.QueryAsync<int>(query, new { UserId = userId });

        var results = new List<OrderDto>();
        foreach (var id in orderIds)
        {
            var order = await HydrateAsync(conn, id, culture);
            if (order != null) results.Add(order);
        }
        return results;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await HydrateAsync(conn, id, culture);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            var souvenirIds = dto.Items.Select(i => i.SouvenirId).ToArray();
            const string priceQuery = """
                SELECT id, price 
                FROM souvenirs 
                WHERE id = ANY(@Ids)
                """;
            var prices = (await conn.QueryAsync<(int Id, decimal Price)>(priceQuery, new { Ids = souvenirIds }, tx))
                .ToDictionary(k => k.Id, v => v.Price);

            decimal total = dto.Items.Sum(i => prices.GetValueOrDefault(i.SouvenirId) * i.Quantity);

            const string orderQuery = """
                INSERT INTO orders (user_id, status, total_amount)
                VALUES (@UserId, 'pending', @TotalAmount)
                RETURNING id
                """;
            int orderId = await conn.ExecuteScalarAsync<int>(orderQuery, new { dto.UserId, TotalAmount = total }, tx);

            const string itemQuery = """
                INSERT INTO order_items (order_id, souvenir_id, quantity, unit_price)
                VALUES (@OrderId, @SouvenirId, @Quantity, @UnitPrice)
                """;
            foreach (var item in dto.Items)
            {
                decimal unitPrice = prices.GetValueOrDefault(item.SouvenirId);
                await conn.ExecuteAsync(itemQuery, new { OrderId = orderId, item.SouvenirId, item.Quantity, UnitPrice = unitPrice }, tx);
            }

            await tx.CommitAsync();
            return (await HydrateAsync(conn, orderId, culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<OrderDto?> PatchStatusAsync(int id, PatchOrderStatusDto dto, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            UPDATE orders 
            SET status = @Status 
            WHERE id = @Id
            """;
        int rows = await conn.ExecuteAsync(query, new { Id = id, dto.Status });
        return rows == 0 ? null : await HydrateAsync(conn, id, culture);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            DELETE FROM orders 
            WHERE id = @Id
            """;
        return await conn.ExecuteAsync(query, new { Id = id }) > 0;
    }
}
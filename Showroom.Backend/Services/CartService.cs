using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

public class CartService : ICartService
{
    private readonly string _connectionString;

    public CartService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    public async Task<int> CreateCartAsync()
    {
        using var conn = Conn();
        const string query = """
            INSERT INTO carts DEFAULT VALUES 
            RETURNING id
            """;
        return await conn.ExecuteScalarAsync<int>(query);
    }

    public async Task<CartDto?> GetByUserAsync(int userId, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT cart_id 
            FROM users 
            WHERE id = @UserId
            """;
        int? cartId = await conn.ExecuteScalarAsync<int?>(query, new { UserId = userId });

        if (cartId is null) return null;

        return await GetByIdAsync(cartId.Value, culture);
    }

    public async Task<CartDto?> GetByIdAsync(int cartId, string culture = "en")
    {
        using var conn = Conn();
        const string cartQuery = """
            SELECT 
                id         AS Id, 
                user_id    AS UserId, 
                created_at AS CreatedAt 
            FROM carts 
            WHERE id = @CartId
            """;
        var cart = await conn.QuerySingleOrDefaultAsync<CartDto>(cartQuery, new { CartId = cartId });

        if (cart == null) return null;

        const string itemsQuery = """
            SELECT
                ci.id                     AS Id, 
                ci.cart_id                AS CartId, 
                ci.souvenir_id            AS SouvenirId,
                COALESCE(t.name, en.name) AS SouvenirName,
                s.price                   AS SouvenirPrice,
                ci.quantity               AS Quantity
            FROM cart_items ci
            JOIN souvenirs s ON s.id = ci.souvenir_id
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE ci.cart_id = @CartId
            """;
        var items = await conn.QueryAsync<CartItemDto>(itemsQuery, new { CartId = cartId, Culture = culture });

        cart.Items = items.ToList();
        return cart;
    }

    public async Task<CartItemDto?> AddItemAsync(AddCartItemDto dto, int userId, string culture = "en")
    {
        using var conn = Conn();

        const string getCartQuery = """
            SELECT cart_id 
            FROM users 
            WHERE id = @UserId
            """;

        var cartId = await conn.ExecuteScalarAsync<int?>(getCartQuery, new { UserId = userId });

        const string query = """
            INSERT INTO cart_items (cart_id, souvenir_id, quantity)
            VALUES (@CartId, @SouvenirId, @Quantity)
            ON CONFLICT (cart_id, souvenir_id) 
            DO UPDATE SET quantity = cart_items.quantity + EXCLUDED.quantity
            """;

        await conn.ExecuteAsync(query, new { CartId = cartId, SouvenirId = dto.SouvenirId, Quantity = dto.Quantity });
        
        const string selectQuery = """
            SELECT
                ci.id                     AS Id, 
                ci.cart_id                AS CartId, 
                ci.souvenir_id            AS SouvenirId,
                COALESCE(t.name, en.name) AS SouvenirName,
                s.price                   AS SouvenirPrice,
                ci.quantity               AS Quantity
            FROM cart_items ci
            JOIN souvenirs s ON s.id = ci.souvenir_id
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE ci.cart_id = @CartId AND ci.souvenir_id = @SouvenirId
            """;
        
        return await conn.QuerySingleOrDefaultAsync<CartItemDto>(selectQuery, new { CartId = cartId, SouvenirId = dto.SouvenirId, Culture = culture });
    }

    public async Task<CartItemDto?> UpdateItemQuantityAsync(int cartId, int souvenirId, PatchCartItemDto dto, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            UPDATE cart_items
            SET quantity = @Quantity
            WHERE cart_id = @CartId AND souvenir_id = @SouvenirId
            """;
        int rows = await conn.ExecuteAsync(query, new { CartId = cartId, SouvenirId = souvenirId, Quantity = dto.Quantity });

        if (rows == 0) return null;

        const string selectQuery = """
            SELECT
                ci.id                     AS Id, 
                ci.cart_id                AS CartId, 
                ci.souvenir_id            AS SouvenirId,
                COALESCE(t.name, en.name) AS SouvenirName,
                s.price                   AS SouvenirPrice,
                ci.quantity               AS Quantity
            FROM cart_items ci
            JOIN souvenirs s ON s.id = ci.souvenir_id
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE ci.cart_id = @CartId AND ci.souvenir_id = @SouvenirId
            """;
        return await conn.QuerySingleOrDefaultAsync<CartItemDto>(selectQuery, new { CartId = cartId, SouvenirId = souvenirId, Culture = culture });
    }

    public async Task<bool> RemoveItemAsync(int userId, int souvenirId)
    {
        using var conn = Conn();

        const string checkUserQuery = """
            SELECT cart_id
            FROM users 
            WHERE id = @UserId
            """;

        int? cartId = await conn.ExecuteScalarAsync<int?>(checkUserQuery, new { UserId = userId });

        if (cartId is null) return false;

        const string query = """
            DELETE FROM cart_items
            WHERE cart_id = @CartId AND souvenir_id = @SouvenirId
            """;
        return await conn.ExecuteAsync(query, new { CartId = cartId, SouvenirId = souvenirId }) > 0;
    }

    public async Task ClearAsync(int cartId)
    {
        using var conn = Conn();
        const string query = """
            DELETE FROM cart_items 
            WHERE cart_id = @CartId
            """;
        await conn.ExecuteAsync(query, new { CartId = cartId });
    }

    public async Task<bool> CheckoutAsync(int userId, bool isPaymentSuccessful)
    {
        if (!isPaymentSuccessful)
        {
            return false;
        }

        using var conn = Conn();
        await conn.OpenAsync();

        using var tx = await conn.BeginTransactionAsync();
        try
        {
            // 1. Recupero gli elementi nel carrello
            const string itemsQuery = """
                SELECT 
                    ci.cart_id     AS CartId,
                    ci.souvenir_id AS SouvenirId, 
                    ci.quantity    AS Quantity, 
                    s.price        AS UnitPrice
                FROM cart_items ci
                JOIN souvenirs s ON s.id = ci.souvenir_id
                JOIN users u ON u.cart_id = ci.cart_id
                WHERE u.id = @UserId
                """;

            var cartItems = (await conn.QueryAsync<CheckoutItemDto>(itemsQuery, new { UserId = userId }, tx)).ToList();

            if (!cartItems.Any()) return false;

            // 2. GESTIONE SCORTE (quantity_available e in_stock)
            const string updateStockQuery = """
                UPDATE souvenirs 
                SET quantity_available = quantity_available - @Quantity,
                    in_stock = CASE WHEN (quantity_available - @Quantity) <= 0 THEN FALSE ELSE TRUE END
                WHERE id = @SouvenirId AND quantity_available >= @Quantity
                """;

            foreach (var item in cartItems)
            {
                var rowsAffected = await conn.ExecuteAsync(updateStockQuery, new { item.Quantity, item.SouvenirId }, tx);

                if (rowsAffected == 0)
                {
                    // Lancia un'eccezione che farà scattare il Rollback
                    throw new InvalidOperationException($"Il prodotto con ID {item.SouvenirId} è esaurito o non ha quantità sufficiente.");
                }
            }

            // 3. Calcolo il totale
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.UnitPrice);
            int cartId = cartItems.First().CartId;

            // 4. CREAZIONE DELL'ORDINE (Ora lo stato è forzato a 'paid' perché arriviamo qui solo se ha pagato)
            const string insertOrderQuery = """
                INSERT INTO orders (user_id, status, total_amount)
                VALUES (@UserId, 'paid', @TotalAmount)
                RETURNING id
                """;
            int orderId = await conn.ExecuteScalarAsync<int>(insertOrderQuery, new { UserId = userId, TotalAmount = totalAmount }, tx);

            // 5. CREAZIONE DEGLI ORDER ITEMS
            const string insertOrderItemsQuery = """
                INSERT INTO order_items (order_id, souvenir_id, quantity, unit_price)
                VALUES (@OrderId, @SouvenirId, @Quantity, @UnitPrice)
                """;

            var orderItemsToInsert = cartItems.Select(item => new
            {
                OrderId = orderId,
                item.SouvenirId,
                item.Quantity,
                item.UnitPrice
            });
            await conn.ExecuteAsync(insertOrderItemsQuery, orderItemsToInsert, tx);

            // 6. SVUOTA IL CARRELLO (Cancelliamo i cart_items associati al cart_id)
            const string clearCartQuery = """
                DELETE FROM cart_items 
                WHERE cart_id = @CartId
                """;
            await conn.ExecuteAsync(clearCartQuery, new { CartId = cartId }, tx);

            // Confermiamo tutte le query
            await tx.CommitAsync();
            return true;
        }
        catch
        {
            // In caso di errore (es. prodotto esaurito in quel millisecondo), 
            // annulliamo tutto. Il carrello rimarrà intatto per il frontend.
            await tx.RollbackAsync();
            throw;
        }
    }
}
using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

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

    public async Task<CartItemDto?> AddItemAsync(int cartId, AddCartItemDto dto, string culture = "en")
    {
        using var conn = Conn();
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

    public async Task<bool> RemoveItemAsync(int cartId, int souvenirId)
    {
        using var conn = Conn();
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
}
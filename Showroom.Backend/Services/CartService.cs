using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════════
//  CART SERVICE
// ══════════════════════════════════════════════════════════════════
public class CartService : ICartService
{
    private readonly string _connectionString;

    public CartService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    // GET cart with items (resolved by user's cart_id)
    public async Task<CartDto?> GetByUserAsync(int userId, string culture = "en")
    {
        using var conn = Conn();

        // 1. resolve cart id (Scalar value non necessita di alias)
        int? cartId = await conn.ExecuteScalarAsync<int?>(
            "SELECT cart_id FROM users WHERE id = @UserId", new { UserId = userId });

        if (cartId is null) return null;

        return await GetByIdAsync(cartId.Value, culture);
    }

    // GET cart with items (by cart id)
    public async Task<CartDto?> GetByIdAsync(int cartId, string culture = "en")
    {
        using var conn = Conn();

        // Aggiunti gli alias anche qui per sicurezza con la tupla
        var cart = await conn.QuerySingleOrDefaultAsync<(int Id, DateTime CreatedAt)>(
            "SELECT id AS Id, created_at AS CreatedAt FROM carts WHERE id = @CartId", new { CartId = cartId });

        if (cart == default) return null;

        var items = await conn.QueryAsync<CartItemDto>("""
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
            ORDER BY ci.id;
            """, new { CartId = cartId, Culture = culture });

        return new CartDto()
        {
            Id = cart.Id, CreatedAt = cart.CreatedAt, Items = items
        };
    }

    // POST — create a bare cart (used during user registration)
    public async Task<int> CreateCartAsync()
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO carts DEFAULT VALUES RETURNING id AS Id");
    }

    // POST — add item (or increment quantity if already in cart)
    public async Task<CartItemDto?> AddItemAsync(int cartId, AddCartItemDto dto, string culture = "en")
    {
        using var conn = Conn();
        int itemId = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO cart_items (cart_id, souvenir_id, quantity)
            VALUES (@CartId, @SouvenirId, @Quantity)
            ON CONFLICT (cart_id, souvenir_id) DO UPDATE
                SET quantity = cart_items.quantity + EXCLUDED.quantity
            RETURNING id AS Id;
            """, new { CartId = cartId, dto.SouvenirId, dto.Quantity });

        return await conn.QuerySingleOrDefaultAsync<CartItemDto>("""
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
            WHERE ci.id = @ItemId;
            """, new { ItemId = itemId, Culture = culture });
    }

    // PATCH — update quantity of a specific cart item
    public async Task<CartItemDto?> UpdateItemQuantityAsync(
        int cartId, int souvenirId, PatchCartItemDto dto, string culture = "en")
    {
        using var conn = Conn();
        int rows = await conn.ExecuteAsync("""
            UPDATE cart_items
            SET quantity = @Quantity
            WHERE cart_id = @CartId AND souvenir_id = @SouvenirId;
            """, new { CartId = cartId, SouvenirId = souvenirId, dto.Quantity });

        if (rows == 0) return null;

        return await conn.QuerySingleOrDefaultAsync<CartItemDto>("""
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
            WHERE ci.cart_id = @CartId AND ci.souvenir_id = @SouvenirId;
            """, new { CartId = cartId, SouvenirId = souvenirId, Culture = culture });
    }

    // DELETE — remove a single item
    public async Task<bool> RemoveItemAsync(int cartId, int souvenirId)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync("""
            DELETE FROM cart_items
            WHERE cart_id = @CartId AND souvenir_id = @SouvenirId;
            """, new { CartId = cartId, SouvenirId = souvenirId }) > 0;
    }

    // DELETE — empty the entire cart (keep the cart row itself)
    public async Task ClearAsync(int cartId)
    {
        using var conn = Conn();
        await conn.ExecuteAsync(
            "DELETE FROM cart_items WHERE cart_id = @CartId", new { CartId = cartId });
    }
}
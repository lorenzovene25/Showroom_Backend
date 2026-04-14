using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Security;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

public class UserService : IUserService
{
    private readonly string _connectionString;
    private readonly IJwtProvider _jwtProvider;

    public UserService(IConfiguration configuration, IJwtProvider jwtProvider)
    {
        _connectionString = configuration.GetConnectionString("db")!;
        _jwtProvider = jwtProvider;
    }

    private NpgsqlConnection Conn() => new(_connectionString);

    private const string SelectFragment = """
        SELECT 
            id         AS Id, 
            first_name AS FirstName, 
            last_name  AS LastName, 
            email      AS Email, 
            is_admin   AS IsAdmin, 
            cart_id    AS CartId, 
            created_at AS CreatedAt
        FROM users
        """;

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            ORDER BY last_name, first_name
            """;
        return await conn.QueryAsync<UserDto>(query);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            WHERE id = @Id
            """;
        return await conn.QuerySingleOrDefaultAsync<UserDto>(query, new { Id = id });
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            WHERE email = @Email
            """;
        return await conn.QuerySingleOrDefaultAsync<UserDto>(query, new { Email = email });
    }

    public async Task<string?> GetPasswordHashAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            SELECT password_hash 
            FROM users 
            WHERE id = @Id
            """;
        return await conn.QuerySingleOrDefaultAsync<string>(query, new { Id = id });
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            const string cartQuery = """
                INSERT INTO carts DEFAULT VALUES RETURNING id
                """;
            int cartId = await conn.ExecuteScalarAsync<int>(cartQuery, null, tx);

            const string userQuery = """
                INSERT INTO users (first_name, last_name, email, password_hash, is_admin, cart_id)
                VALUES (@FirstName, @LastName, @Email, @PasswordHash, @IsAdmin, @CartId)
                RETURNING id
                """;
            int id = await conn.ExecuteScalarAsync<int>(userQuery, new { dto.FirstName, dto.LastName, dto.Email, PasswordHash = dto.Password, dto.IsAdmin, CartId = cartId }, tx);

            await tx.CommitAsync();
            return (await GetByIdAsync(id))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        using var conn = Conn();
        const string query = """
            UPDATE users 
            SET first_name = @FirstName, 
                last_name = @LastName, 
                email = @Email, 
                is_admin = @IsAdmin, 
                password_hash = COALESCE(@PasswordHash, password_hash),
                updated_at = NOW()
            WHERE id = @Id
            """;
        int rows = await conn.ExecuteAsync(query, new { Id = id, dto.FirstName, dto.LastName, dto.Email, dto.IsAdmin, dto.PasswordHash });
        return rows == 0 ? null : await GetByIdAsync(id);
    }

    public async Task<UserDto?> PatchAsync(int id, PatchUserDto dto)
    {
        using var conn = Conn();
        var sets = new List<string> { "updated_at = NOW()" };
        var p = new DynamicParameters(new { Id = id });

        if (dto.FirstName != null) { sets.Add("first_name = @FirstName"); p.Add("FirstName", dto.FirstName); }
        if (dto.LastName != null) { sets.Add("last_name = @LastName"); p.Add("LastName", dto.LastName); }
        if (dto.Email != null) { sets.Add("email = @Email"); p.Add("Email", dto.Email); }
        if (dto.IsAdmin != null) { sets.Add("is_admin = @IsAdmin"); p.Add("IsAdmin", dto.IsAdmin); }
        if (dto.PasswordHash != null) { sets.Add("password_hash = @PasswordHash"); p.Add("PasswordHash", dto.PasswordHash); }

        string query = $"""
            UPDATE users 
            SET {string.Join(", ", sets)} 
            WHERE id = @Id
            """;
        int rows = await conn.ExecuteAsync(query, p);
        return rows == 0 ? null : await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            DELETE FROM users 
            WHERE id = @Id
            """;
        return await conn.ExecuteAsync(query, new { Id = id }) > 0;
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                t.id            AS Id, 
                t.exhibition_id AS ExhibitionId, 
                COALESCE(et.title, en.title) AS ExhibitionTitle,
                t.tier_id       AS TierId, 
                COALESCE(ttt.name, ttten.name) AS TierName,
                tt.price        AS TierPrice,    -- Corretto l'alias per matchare il tuo DTO e non stampare 0!
                t.user_id       AS UserId, 
                t.visit_date    AS VisitDate,
                t.time_slot_id  AS TimeSlotId, 
                ts.start_time   AS SlotStartTime, -- Recupero l'orario di inizio
                ts.end_time     AS SlotEndTime,   -- Recupero l'orario di fine
                t.purchased_at  AS PurchasedAt
            FROM tickets t
            
            -- Join per recuperare il prezzo base
            INNER JOIN ticket_tiers tt ON tt.id = t.tier_id
            
            -- Join per recuperare gli orari dallo slot
            INNER JOIN exhibition_time_slots ts ON ts.id = t.time_slot_id
            
            -- Join per il nome della Mostra
            LEFT JOIN exhibitions e ON e.id = t.exhibition_id
            LEFT JOIN exhibition_translations et ON et.exhibition_id = e.id AND et.language_code = @Culture
            LEFT JOIN exhibition_translations en ON en.exhibition_id = e.id AND en.language_code = 'en'
            
            -- Join per il nome tradotto del Tipo di Biglietto (Tier)
            LEFT JOIN ticket_tier_translations ttt ON ttt.ticket_tier_id = t.tier_id AND ttt.language_code = @Culture
            LEFT JOIN ticket_tier_translations ttten ON ttten.ticket_tier_id = t.tier_id AND ttten.language_code = 'en'
            
            WHERE t.user_id = @UserId
        """;

        return await conn.QueryAsync<TicketDto>(query, new { UserId = id, Culture = culture });
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                id           AS Id, 
                user_id      AS UserId, 
                status       AS Status, 
                total_amount AS TotalAmount, 
                created_at   AS CreatedAt 
            FROM orders 
            WHERE user_id = @UserId 
            ORDER BY created_at DESC
            """;
        return await conn.QueryAsync<OrderDto>(query, new { UserId = id });
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id, int orderId, string culture = "en")
    {
        using var conn = Conn();
        const string orderQuery = """
            SELECT 
                id           AS Id, 
                user_id      AS UserId, 
                status       AS Status, 
                total_amount AS TotalAmount, 
                created_at   AS CreatedAt 
            FROM orders 
            WHERE id = @OrderId AND user_id = @UserId
            """;
        var order = await conn.QuerySingleOrDefaultAsync<OrderDto>(orderQuery, new { OrderId = orderId, UserId = id });

        if (order != null)
        {
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
            order.Items = items.ToList();
        }

        return order;
    }

    public async Task<CartDto?> GetCartAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT cart_id 
            FROM users 
            WHERE id = @UserId
            """;
        int? cartId = await conn.ExecuteScalarAsync<int?>(query, new { UserId = id });

        if (cartId == null) return null;

        const string cartQuery = """
            SELECT 
                id         AS Id
            FROM carts
            WHERE id = @CartId
            """;
        var cart = await conn.QuerySingleOrDefaultAsync<CartDto>(cartQuery, new { CartId = cartId });

        if (cart != null)
        {
            const string itemsQuery = """
                SELECT
                    ci.id                     AS Id, 
                    ci.cart_id                AS CartId, 
                    ci.souvenir_id            AS SouvenirId,
                    COALESCE(t.name, en.name) AS SouvenirName,
                    s.price                   AS SouvenirPrice,
                    ci.quantity               AS Quantity,
                    s.image_url               AS SouvenirImageUrl
                FROM cart_items ci
                JOIN souvenirs s ON s.id = ci.souvenir_id
                LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
                LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
                WHERE ci.cart_id = @CartId
                """;
            var items = await conn.QueryAsync<CartItemDto>(itemsQuery, new { CartId = cartId, Culture = culture });
            cart.Items = items.ToList();
        }

        return cart;
    }

    public async Task<string?> LoginAsync(LoginUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password)) return null;

        var user = await GetByEmailAsync(dto.Email);
        if (user == null) return null;

        var hash = await GetPasswordHashAsync(user.Id);
        if (hash == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, hash)) return null;

        return _jwtProvider.GenerateToken(user);
    }

    public async Task<bool> RegisterAsync(CreateUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password)) return false;

        var existingUser = await GetByEmailAsync(dto.Email);
        if (existingUser != null) return false;

        var createUserDto = new CreateUserDto
        {
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsAdmin = false
        };

        await CreateAsync(createUserDto);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordUserDto dto)
    {
        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.OldPassword) || string.IsNullOrEmpty(dto.NewPassword)) return false;

        var user = await GetByEmailAsync(dto.Email);
        if (user == null) return false;

        var currentHash = await GetPasswordHashAsync(user.Id);
        if (currentHash == null || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, currentHash)) return false;

        var newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        var updatedUser = await PatchAsync(user.Id, new PatchUserDto { PasswordHash = newHash });

        return updatedUser != null;
    }
}
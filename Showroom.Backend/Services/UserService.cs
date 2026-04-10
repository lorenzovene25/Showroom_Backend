using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public class UserService : IUserService
{
    private readonly string _connectionString;

    public UserService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

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

    // GET ALL (admin)
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        using var conn = Conn();
        return await conn.QueryAsync<UserDto>(SelectFragment + " ORDER BY last_name, first_name");
    }

    // GET BY ID
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<UserDto>(
            SelectFragment + " WHERE id = @Id", new { Id = id });
    }

    // GET BY EMAIL (useful for login / duplicate check)
    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<UserDto>(
            SelectFragment + " WHERE email = @Email", new { Email = email });
    }

    // GET password hash — kept separate so it is never accidentally
    // included in a UserDto returned to the client
    public async Task<string?> GetPasswordHashAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteScalarAsync<string?>(
            "SELECT password_hash FROM users WHERE id = @Id", new { Id = id });
    }

    // POST — creates a cart and the user atomically
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Aggiunto AS Id
            int cartId = await conn.ExecuteScalarAsync<int>(
                "INSERT INTO carts DEFAULT VALUES RETURNING id AS Id", transaction: tx);

            // Aggiunto AS Id
            int userId = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO users (first_name, last_name, email, password_hash, is_admin, cart_id)
                VALUES (@FirstName, @LastName, @Email, @PasswordHash, @IsAdmin, @CartId)
                RETURNING id AS Id;
                """,
                new
                {
                    dto.FirstName,
                    dto.LastName,
                    dto.Email,
                    dto.PasswordHash,
                    dto.IsAdmin,
                    CartId = cartId
                },
                tx);

            await tx.CommitAsync();
            return (await GetByIdAsync(userId))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PUT — full replacement (password unchanged when PasswordHash is null)
    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        using var conn = Conn();

        var sets = new List<string>
        {
            "first_name = @FirstName",
            "last_name  = @LastName",
            "email      = @Email",
            "is_admin   = @IsAdmin",
            "updated_at = NOW()"
        };
        var p = new DynamicParameters(new
        {
            Id = id,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.IsAdmin
        });

        if (dto.PasswordHash != null)
        {
            sets.Add("password_hash = @PasswordHash");
            p.Add("PasswordHash", dto.PasswordHash);
        }

        int rows = await conn.ExecuteAsync(
            $"UPDATE users SET {string.Join(", ", sets)} WHERE id = @Id", p);

        return rows == 0 ? null : await GetByIdAsync(id);
    }

    // PATCH — only non-null fields are written
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

        int rows = await conn.ExecuteAsync(
            $"UPDATE users SET {string.Join(", ", sets)} WHERE id = @Id", p);

        return rows == 0 ? null : await GetByIdAsync(id);
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM users WHERE id = @Id", new { Id = id }) > 0;
    }

    public Task<IEnumerable<TicketDto>> GetTicketsAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderDto>> GetOrdersAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<OrderDto?> GetOrderByIdAsync(int id, int orderId)
    {
        throw new NotImplementedException();
    }

    public Task<CartDto?> GetCartAsync(int id)
    {
        throw new NotImplementedException();
    }
}
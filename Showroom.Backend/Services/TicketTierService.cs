using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════════
//  TICKET TIER SERVICE
// ══════════════════════════════════════════════════════════════════
public class TicketTierService : ITicketTierService
{
    private readonly string _connectionString;

    public TicketTierService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    private const string SelectFragment = """
        SELECT
            tt.id                                   AS Id, 
            tt.type                                 AS Type, 
            tt.price                                AS Price,
            COALESCE(t.name,        en.name)        AS Name,
            COALESCE(t.description, en.description) AS Description
        FROM ticket_tiers tt
        LEFT JOIN ticket_tier_translations t  ON t.ticket_tier_id = tt.id AND t.language_code = @Culture
        LEFT JOIN ticket_tier_translations en ON en.ticket_tier_id = tt.id AND en.language_code = 'en'
        """;

    // GET ALL
    public async Task<IEnumerable<TicketTierDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<TicketTierDto>(
            SelectFragment + " ORDER BY tt.price",
            new { Culture = culture });
    }

    // GET BY ID
    public async Task<TicketTierDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<TicketTierDto>(
            SelectFragment + " WHERE tt.id = @Id",
            new { Id = id, Culture = culture });
    }

    // POST
    public async Task<TicketTierDto> CreateAsync(CreateTicketTierDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int id = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO ticket_tiers (type, price)
                VALUES (@Type, @Price)
                RETURNING id AS Id;
                """, dto, tx);

            await UpsertTranslationAsync(conn, tx, id, dto.Culture, dto.Name, dto.Description);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PUT
    public async Task<TicketTierDto?> UpdateAsync(int id, UpdateTicketTierDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int rows = await conn.ExecuteAsync("""
                UPDATE ticket_tiers SET type = @Type, price = @Price WHERE id = @Id;
                """, new { Id = id, dto.Type, dto.Price }, tx);

            if (rows == 0) { await tx.RollbackAsync(); return null; }

            await UpsertTranslationAsync(conn, tx, id, culture, dto.Name, dto.Description);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PATCH
    public async Task<TicketTierDto?> PatchAsync(int id, PatchTicketTierDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var sets = new List<string>();
            var p = new DynamicParameters(new { Id = id });

            if (dto.Type != null) { sets.Add("type = @Type"); p.Add("Type", dto.Type); }
            if (dto.Price != null) { sets.Add("price = @Price"); p.Add("Price", dto.Price); }

            if (sets.Count > 0)
            {
                int rows = await conn.ExecuteAsync(
                    $"UPDATE ticket_tiers SET {string.Join(", ", sets)} WHERE id = @Id", p, tx);
                if (rows == 0) { await tx.RollbackAsync(); return null; }
            }
            else
            {
                bool exists = await conn.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM ticket_tiers WHERE id = @Id)", new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            if (dto.Name != null || dto.Description != null)
                await PatchTranslationAsync(conn, tx, id, culture, dto.Name, dto.Description);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM ticket_tiers WHERE id = @Id", new { Id = id }) > 0;
    }

    private static async Task UpsertTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int tierId, string culture, string? name, string? description)
    {
        await conn.ExecuteAsync("""
            INSERT INTO ticket_tier_translations (ticket_tier_id, language_code, name, description)
            VALUES (@TierId, @Culture, @Name, @Description)
            ON CONFLICT (ticket_tier_id, language_code) DO UPDATE SET
                name        = EXCLUDED.name,
                description = EXCLUDED.description;
            """,
            new { TierId = tierId, Culture = culture, Name = name, Description = description }, tx);
    }

    private static async Task PatchTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int tierId, string culture, string? name, string? description)
    {
        await conn.ExecuteAsync("""
            INSERT INTO ticket_tier_translations (ticket_tier_id, language_code, name, description)
            VALUES (@TierId, @Culture, @Name, @Description)
            ON CONFLICT (ticket_tier_id, language_code) DO UPDATE SET
                name        = COALESCE(EXCLUDED.name,        ticket_tier_translations.name),
                description = COALESCE(EXCLUDED.description, ticket_tier_translations.description);
            """,
            new { TierId = tierId, Culture = culture, Name = name, Description = description }, tx);
    }
}
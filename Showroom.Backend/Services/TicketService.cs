using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════════
//  TICKET SERVICE
// ══════════════════════════════════════════════════════════════════
public class TicketService : ITicketService
{
    private readonly string _connectionString;

    public TicketService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    // Joins exhibition title, tier name/price, and slot times into a flat DTO.
    private const string SelectFragment = """
    SELECT 
        tk.id                          AS Id, 
        tk.exhibition_id               AS ExhibitionId, 
        COALESCE(et.title, eten.title) AS ExhibitionTitle, 
        tk.tier_id                     AS TierId, 
        COALESCE(ttt.name, tttn.name)  AS TierName, 
        tt.price                       AS TierPrice, 
        tk.user_id                     AS UserId, 
        tk.visit_date                  AS VisitDate, 
        tk.time_slot_id                AS TimeSlotId, 
        ts.start_time                  AS SlotStartTime, 
        ts.end_time                    AS SlotEndTime, 
        tk.purchased_at                AS PurchasedAt 
    FROM tickets tk 
    JOIN exhibitions e  ON e.id  = tk.exhibition_id 
    JOIN ticket_tiers tt ON tt.id = tk.tier_id 
    JOIN exhibition_time_slots ts ON ts.id = tk.time_slot_id 
    LEFT JOIN exhibition_translations et   ON et.exhibition_id  = e.id  AND et.language_code  = @Culture 
    LEFT JOIN exhibition_translations eten ON eten.exhibition_id = e.id  AND eten.language_code = 'en' 
    LEFT JOIN ticket_tier_translations ttt  ON ttt.ticket_tier_id = tt.id AND ttt.language_code  = @Culture 
    LEFT JOIN ticket_tier_translations tttn ON tttn.ticket_tier_id = tt.id AND tttn.language_code = 'en' 
    """;

    // GET ALL (admin)
    public async Task<IEnumerable<TicketDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<TicketDto>(
            SelectFragment + " ORDER BY tk.purchased_at DESC",
            new { Culture = culture });
    }

    // GET BY USER
    public async Task<IEnumerable<TicketDto>> GetByUserAsync(int userId, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<TicketDto>(
            SelectFragment + " WHERE tk.user_id = @UserId ORDER BY tk.visit_date DESC",
            new { UserId = userId, Culture = culture });
    }

    // GET BY ID
    public async Task<TicketDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<TicketDto>(
            SelectFragment + " WHERE tk.id = @Id",
            new { Id = id, Culture = culture });
    }

    // GET BY EXHIBITION
    public async Task<IEnumerable<TicketDto>> GetByExhibitionAsync(int exhibitionId, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<TicketDto>(
            SelectFragment + " WHERE tk.exhibition_id = @ExhibitionId ORDER BY tk.visit_date",
            new { ExhibitionId = exhibitionId, Culture = culture });
    }

    // POST
    public async Task<TicketDto> CreateAsync(CreateTicketDto dto, string culture = "en")
    {
        using var conn = Conn();
        // Aggiunto l'alias AS Id qui
        int id = await conn.ExecuteScalarAsync<int>("""
        INSERT INTO tickets (exhibition_id, tier_id, user_id, visit_date, time_slot_id)
        VALUES (@ExhibitionId, @TierId, @UserId, @VisitDate, @TimeSlotId)
        RETURNING id AS Id;
        """, dto);
        return (await GetByIdAsync(id, culture))!;
    }

    // DELETE  (tickets cannot be edited once issued, only cancelled = deleted)
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM tickets WHERE id = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<TicketTierDto>> GetAllTicketTiersAsync(string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                tt.id               AS Id,
                tt.type             AS Type,
                tt.price            AS Price,
                ttt.name            AS Name,
                ttt.description     AS Description
            FROM public.ticket_tiers tt
            JOIN public.ticket_tier_translations ttt ON tt.id = ttt.ticket_tier_id 
            WHERE ttt.language_code = @Culture
            """;
        return await conn.QueryAsync<TicketTierDto>(query,
            new { Culture = culture });
    }
}
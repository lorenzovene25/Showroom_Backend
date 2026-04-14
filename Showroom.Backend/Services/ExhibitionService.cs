using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

public class ExhibitionService : IExhibitionService
{
    private readonly string _connectionString;

    public ExhibitionService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    // Costante base per evitare di ripetere i campi nelle query di selezione
    private const string SelectFragment = """
        SELECT 
            e.id                                    AS Id,
            e.name                                  AS Name,
            e.location                              AS Location,
            e.maps_url                              AS MapsUrl,
            e.status                                AS Status,
            e.start_date                            AS StartDate,
            e.end_date                              AS EndDate,
            e.image_url                             AS ImageUrl,
            COALESCE(t.title,       en.title)       AS Title,
            COALESCE(t.description, en.description) AS Description
        FROM exhibitions e
        LEFT JOIN exhibition_translations t  ON t.exhibition_id = e.id AND t.language_code = @Culture
        LEFT JOIN exhibition_translations en ON en.exhibition_id = e.id AND en.language_code = 'en'
        """;

    // ══════════════════════════════════════════════════════════════
    //  EXHIBITIONS
    // ══════════════════════════════════════════════════════════════

    public async Task<IEnumerable<ExhibitionDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            ORDER BY e.start_date DESC
            """;
        return await conn.QueryAsync<ExhibitionDto>(query, new { Culture = culture });
    }

    public async Task<IEnumerable<ExhibitionDto>> GetByStatusAsync(string status, string culture = "en")
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            WHERE e.status = @Status 
            ORDER BY e.start_date DESC
            """;
        return await conn.QueryAsync<ExhibitionDto>(query, new { Status = status, Culture = culture });
    }

    public async Task<ExhibitionDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            WHERE e.id = @Id
            """;
        return await conn.QuerySingleOrDefaultAsync<ExhibitionDto>(query, new { Id = id, Culture = culture });
    }

    public async Task<ExhibitionDto> CreateAsync(CreateExhibitionDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            const string query = """
                INSERT INTO exhibitions (name, location, maps_url, status, start_date, end_date, image_url)
                VALUES (@Name, @Location, @MapsUrl, @Status, @StartDate, @EndDate, @ImageUrl) 
                RETURNING id
                """;

            int id = await conn.ExecuteScalarAsync<int>(query, dto, tx);

            await UpsertTranslationAsync(conn, tx, id, dto.Culture, dto.Title, dto.Description);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<ExhibitionDto?> UpdateAsync(int id, UpdateExhibitionDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            const string query = """
                UPDATE exhibitions 
                SET name = @Name, 
                    location = @Location, 
                    maps_url = @MapsUrl, 
                    status = @Status, 
                    start_date = @StartDate, 
                    end_date = @EndDate, 
                    image_url = @ImageUrl 
                WHERE id = @Id
                """;

            int rows = await conn.ExecuteAsync(query,
                new { Id = id, dto.Name, dto.Location, dto.MapsUrl, dto.Status, dto.StartDate, dto.EndDate, dto.ImageUrl }, tx);

            if (rows == 0) return null;

            await UpsertTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<ExhibitionDto?> PatchAsync(int id, PatchExhibitionDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            var sets = new List<string>();
            var p = new DynamicParameters(new { Id = id });

            if (dto.Name != null) { sets.Add("name = @Name"); p.Add("Name", dto.Name); }
            if (dto.Location != null) { sets.Add("location = @Location"); p.Add("Location", dto.Location); }
            if (dto.MapsUrl != null) { sets.Add("maps_url = @MapsUrl"); p.Add("MapsUrl", dto.MapsUrl); }
            if (dto.Status != null) { sets.Add("status = @Status"); p.Add("Status", dto.Status); }
            if (dto.StartDate != null) { sets.Add("start_date = @StartDate"); p.Add("StartDate", dto.StartDate); }
            if (dto.EndDate != null) { sets.Add("end_date = @EndDate"); p.Add("EndDate", dto.EndDate); }
            if (dto.ImageUrl != null) { sets.Add("image_url = @ImageUrl"); p.Add("ImageUrl", dto.ImageUrl); }

            if (sets.Count > 0)
            {
                // Uso l'interpolazione $""" per iniettare dinamicamente il JOIN dei campi
                string query = $"""
                    UPDATE exhibitions 
                    SET {string.Join(", ", sets)} 
                    WHERE id = @Id
                    """;
                await conn.ExecuteAsync(query, p, tx);
            }
            else
            {
                // Se non c'è nulla da aggiornare in questa tabella, controllo almeno che esista
                const string existsQuery = """
                    SELECT EXISTS(SELECT 1 FROM exhibitions WHERE id = @Id)
                    """;
                bool exists = await conn.ExecuteScalarAsync<bool>(existsQuery, new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            if (dto.Title != null || dto.Description != null)
                await PatchTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            DELETE FROM exhibitions 
            WHERE id = @Id
            """;
        return await conn.ExecuteAsync(query, new { Id = id }) > 0;
    }

    // ── PRIVATE HELPERS PER LE TRADUZIONI ──────────────────────────────

    private async Task UpsertTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int exId, string lang, string? title, string? desc)
    {
        const string query = """
            INSERT INTO exhibition_translations (exhibition_id, language_code, title, description)
            VALUES (@ExId, @Lang, @Title, @Desc)
            ON CONFLICT (exhibition_id, language_code) DO UPDATE SET
                title = EXCLUDED.title, 
                description = EXCLUDED.description
            """;
        await conn.ExecuteAsync(query, new { ExId = exId, Lang = lang, Title = title, Desc = desc }, tx);
    }

    private async Task PatchTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int exId, string lang, string? title, string? desc)
    {
        const string query = """
            INSERT INTO exhibition_translations (exhibition_id, language_code, title, description)
            VALUES (@ExId, @Lang, @Title, @Desc)
            ON CONFLICT (exhibition_id, language_code) DO UPDATE SET
                title = COALESCE(EXCLUDED.title, exhibition_translations.title),
                description = COALESCE(EXCLUDED.description, exhibition_translations.description)
            """;
        await conn.ExecuteAsync(query, new { ExId = exId, Lang = lang, Title = title, Desc = desc }, tx);
    }

    public async Task<IEnumerable<ArtworkDto>> GetAllArtworksAsync(int exhibitionId, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                a.id                                                AS Id, 
                a.archive_id                                        AS ArchiveId,
                a.name                                              AS Name, 
                a.year                                              AS Year, 
                a.dimensions                                        AS Dimensions,
                a.image_url                                         AS ImageUrl, 
                COALESCE(t.title, en.title)                         AS Title,
                COALESCE(t.description, en.description)             AS Description,
                COALESCE(t.historical_period, en.historical_period) AS HistoricalPeriod,
                COALESCE(t.support, en.support)                     AS Support,
                COALESCE(t.camera, en.camera)                       AS Camera
            FROM artworks a
            JOIN artwork_exhibitions ae ON ae.artwork_id = a.id
            LEFT JOIN artwork_translations t ON t.artwork_id = a.id AND t.language_code = @Culture
            LEFT JOIN artwork_translations en ON en.artwork_id = a.id AND en.language_code = 'en'
            WHERE ae.exhibition_id = @ExhibitionId
            ORDER BY a.name
            """;
        return await conn.QueryAsync<ArtworkDto>(query, new { ExhibitionId = exhibitionId, Culture = culture });
    }

    public async Task<IEnumerable<ExhibitionTimeSlotDto>> GetAllTimeSlotsAsync(int exhibitionId, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                id              AS Id, 
                exhibition_id   AS ExhibitionId, 
                days_of_week    AS DaysOfWeek, 
                start_time      AS StartTime, 
                end_time        AS EndTime, 
                max_capacity    AS MaxCapacity 
            FROM exhibition_time_slots 
            WHERE exhibition_id = @ExId
            ORDER BY start_time ASC
            """;
        return await conn.QueryAsync<ExhibitionTimeSlotDto>(query, new { ExId = exhibitionId });
    }
}
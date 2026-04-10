using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public class ExhibitionService : IExhibitionService
{
    private readonly string _connectionString;

    public ExhibitionService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

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

    // GET ALL
    public async Task<IEnumerable<ExhibitionDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<ExhibitionDto>(
            SelectFragment + " ORDER BY e.start_date DESC",
            new { Culture = culture });
    }

    // GET ALL by status  (ongoing | past | upcoming)
    public async Task<IEnumerable<ExhibitionDto>> GetByStatusAsync(string status, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<ExhibitionDto>(
            SelectFragment + " WHERE e.status = @Status ORDER BY e.start_date DESC",
            new { Status = status, Culture = culture });
    }

    // GET BY ID
    public async Task<ExhibitionDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<ExhibitionDto>(
            SelectFragment + " WHERE e.id = @Id",
            new { Id = id, Culture = culture });
    }

    // POST
    public async Task<ExhibitionDto> CreateAsync(CreateExhibitionDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int id = await conn.ExecuteScalarAsync<int>(
                """
                INSERT INTO exhibitions (name, location, maps_url, status, start_date, end_date, image_url)
                VALUES (@Name, @Location, @MapsUrl, @Status, @StartDate, @EndDate, @ImageUrl)
                RETURNING id AS Id;
                """, dto, tx);

            // Adesso chiama il metodo privato definito qui sotto!
            await UpsertTranslationAsync(conn, tx, id, dto.Culture, dto.Title, dto.Description);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PUT
    public async Task<ExhibitionDto?> UpdateAsync(int id, UpdateExhibitionDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Rimosso il RETURNING id AS Id, inutile con ExecuteAsync
            int rows = await conn.ExecuteAsync("""
            UPDATE exhibitions
            SET name = @Name, 
                location = @Location, 
                maps_url = @MapsUrl,
                status = @Status, 
                start_date = @StartDate, 
                end_date = @EndDate,
                image_url = @ImageUrl
            WHERE id = @Id;
            """, new
            {
                Id = id,
                dto.Name,
                dto.Location,
                dto.MapsUrl,
                dto.Status,
                dto.StartDate,
                dto.EndDate,
                dto.ImageUrl
            }, tx);

            if (rows == 0) { await tx.RollbackAsync(); return null; }

            await UpsertTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PATCH
    public async Task<ExhibitionDto?> PatchAsync(int id, PatchExhibitionDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
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
                int rows = await conn.ExecuteAsync(
                    $"UPDATE exhibitions SET {string.Join(", ", sets)} WHERE id = @Id", p, tx);
                if (rows == 0) { await tx.RollbackAsync(); return null; }
            }
            else
            {
                bool exists = await conn.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM exhibitions WHERE id = @Id)", new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            if (dto.Title != null || dto.Description != null)
                await PatchTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description);

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
            "DELETE FROM exhibitions WHERE id = @Id", new { Id = id }) > 0;
    }

    // ── PRIVATE HELPERS PER LE TRADUZIONI ──────────────────────────────
    // Spostati qui da TimeSlotService perché è qui che servono!

    private static async Task UpsertTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int exhibitionId, string culture,
        string? title, string? description)
    {
        await conn.ExecuteAsync("""
            INSERT INTO exhibition_translations (exhibition_id, language_code, title, description)
            VALUES (@ExhibitionId, @Culture, @Title, @Description)
            ON CONFLICT (exhibition_id, language_code) DO UPDATE SET
                title       = EXCLUDED.title,
                description = EXCLUDED.description;
            """,
            new
            {
                ExhibitionId = exhibitionId,
                Culture = culture,
                Title = title,
                Description = description
            }, tx);
    }

    private static async Task PatchTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int exhibitionId, string culture,
        string? title, string? description)
    {
        await conn.ExecuteAsync("""
            INSERT INTO exhibition_translations (exhibition_id, language_code, title, description)
            VALUES (@ExhibitionId, @Culture, @Title, @Description)
            ON CONFLICT (exhibition_id, language_code) DO UPDATE SET
                title       = COALESCE(EXCLUDED.title,       exhibition_translations.title),
                description = COALESCE(EXCLUDED.description, exhibition_translations.description);
            """,
            new
            {
                ExhibitionId = exhibitionId,
                Culture = culture,
                Title = title,
                Description = description
            }, tx);
    }

}
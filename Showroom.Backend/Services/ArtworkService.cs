using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public class ArtworkService : IArtworkService
{
    private readonly string _connectionString;

    public ArtworkService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    // Shared SELECT fragment — always LEFT JOINs the requested culture with
    // English as fallback (COALESCE). When culture='en' both joins hit the
    // same rows; COALESCE is a no-op and performance is identical.
    private const string SelectFragment = """
        SELECT
            a.id                                                as Id, 
            a.archive_id                                        AS ArchiveId, 
            a.name                                              as Name, 
            a.year                                              as Year, 
            a.dimensions                                        AS Dimensions, 
            a.image_url                                         as ImageUrl,
            COALESCE(t.title,             en.title)             AS Title,
            COALESCE(t.description,       en.description)       AS Description,
            COALESCE(t.historical_period, en.historical_period) AS HistoricalPeriod,
            COALESCE(t.support,           en.support)           AS Support,
            COALESCE(t.camera,            en.camera)            AS Camera
        FROM artworks a
        LEFT JOIN artwork_translations t  ON t.artwork_id = a.id AND t.language_code = @Culture
        LEFT JOIN artwork_translations en ON en.artwork_id = a.id AND en.language_code = 'en'
        """;

    // ── GET ALL ──────────────────────────────────────────────────────────
    public async Task<IEnumerable<ArtworkDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<ArtworkDto>(
            SelectFragment + " ORDER BY a.name",
            new { Culture = culture });
    }

    // ── GET BY ID ────────────────────────────────────────────────────────
    public async Task<ArtworkDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<ArtworkDto>(
            SelectFragment + " WHERE a.id = @Id",
            new { Id = id, Culture = culture });
    }

    // ── POST ─────────────────────────────────────────────────────────────
    public async Task<ArtworkDto> CreateAsync(CreateArtworkDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO artworks (archive_id, name, year, dimensions, image_url)
            VALUES (@ArchiveId, @Name, @Year, @Dimensions, @ImageUrl)
            RETURNING id;
            """, dto, tx);

            await UpsertTranslationAsync(conn, tx, id, dto.Culture,
                dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // ── PUT ──────────────────────────────────────────────────────────────
    public async Task<ArtworkDto?> UpdateAsync(int id, UpdateArtworkDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int rows = await conn.ExecuteAsync("""
                UPDATE artworks
                SET archive_id = @ArchiveId, name = @Name, year = @Year,
                    dimensions = @Dimensions, image_url = @ImageUrl
                WHERE id = @Id;
                """, new { Id = id, dto.ArchiveId, dto.Name, dto.Year, dto.Dimensions, dto.ImageUrl }, tx);

            if (rows == 0) { await tx.RollbackAsync(); return null; }

            await UpsertTranslationAsync(conn, tx, id, culture,
                dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // ── PATCH ────────────────────────────────────────────────────────────
    // Only non-null fields are written. Translation uses COALESCE upsert so
    // existing values are preserved when the corresponding field is null.
    public async Task<ArtworkDto?> PatchAsync(int id, PatchArtworkDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // --- main table ---
            var sets = new List<string>();
            var p = new DynamicParameters(new { Id = id });

            if (dto.ArchiveId != null) { sets.Add("archive_id = @ArchiveId"); p.Add("ArchiveId", dto.ArchiveId); }
            if (dto.Name != null) { sets.Add("name = @Name"); p.Add("Name", dto.Name); }
            if (dto.Year != null) { sets.Add("year = @Year"); p.Add("Year", dto.Year); }
            if (dto.Dimensions != null) { sets.Add("dimensions = @Dimensions"); p.Add("Dimensions", dto.Dimensions); }
            if (dto.ImageUrl != null) { sets.Add("image_url = @ImageUrl"); p.Add("ImageUrl", dto.ImageUrl); }

            if (sets.Count > 0)
            {
                int rows = await conn.ExecuteAsync(
                    $"UPDATE artworks SET {string.Join(", ", sets)} WHERE id = @Id", p, tx);
                if (rows == 0) { await tx.RollbackAsync(); return null; }
            }
            else
            {
                bool exists = await conn.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM artworks WHERE id = @Id)", new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            // --- translation table (COALESCE keeps existing value when null) ---
            bool hasTransFields = dto.Title != null || dto.Description != null ||
                                  dto.HistoricalPeriod != null || dto.Support != null || dto.Camera != null;
            if (hasTransFields)
                await PatchTranslationAsync(conn, tx, id, culture,
                    dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // ── DELETE ───────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM artworks WHERE id = @Id", new { Id = id }) > 0;
    }

    // ── ARTWORK ↔ EXHIBITION LINKS ───────────────────────────────────────

    // POST  /artworks/{artworkId}/exhibitions/{exhibitionId}
    public async Task<bool> LinkExhibitionAsync(int artworkId, int exhibitionId)
    {
        using var conn = Conn();
        int rows = await conn.ExecuteAsync("""
            INSERT INTO artwork_exhibitions (artwork_id, exhibition_id)
            VALUES (@ArtworkId, @ExhibitionId)
            ON CONFLICT DO NOTHING;
            """, new { ArtworkId = artworkId, ExhibitionId = exhibitionId });
        return rows > 0;
    }

    // DELETE  /artworks/{artworkId}/exhibitions/{exhibitionId}
    public async Task<bool> UnlinkExhibitionAsync(int artworkId, int exhibitionId)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync("""
            DELETE FROM artwork_exhibitions
            WHERE artwork_id = @ArtworkId AND exhibition_id = @ExhibitionId;
            """, new { ArtworkId = artworkId, ExhibitionId = exhibitionId }) > 0;
    }

    // GET  /exhibitions/{exhibitionId}/artworks
    public async Task<IEnumerable<ArtworkDto>> GetByExhibitionAsync(int exhibitionId, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<ArtworkDto>(
            SelectFragment + """
             INNER JOIN artwork_exhibitions ae ON ae.artwork_id = a.id
             WHERE ae.exhibition_id = @ExhibitionId
             ORDER BY a.name;
            """, new { ExhibitionId = exhibitionId, Culture = culture });
    }

    // ── PRIVATE HELPERS ──────────────────────────────────────────────────

    // Full upsert — used by POST and PUT (overwrites all translation fields)
    private static async Task UpsertTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int artworkId, string culture,
        string? title, string? description,
        string? historicalPeriod, string? support, string? camera)
    {
        await conn.ExecuteAsync("""
            INSERT INTO artwork_translations
                (artwork_id, language_code, title, description, historical_period, support, camera)
            VALUES
                (@ArtworkId, @Culture, @Title, @Description, @HistoricalPeriod, @Support, @Camera)
            ON CONFLICT (artwork_id, language_code) DO UPDATE SET
                title             = EXCLUDED.title,
                description       = EXCLUDED.description,
                historical_period = EXCLUDED.historical_period,
                support           = EXCLUDED.support,
                camera            = EXCLUDED.camera;
            """,
            new
            {
                ArtworkId = artworkId,
                Culture = culture,
                Title = title,
                Description = description,
                HistoricalPeriod = historicalPeriod,
                Support = support,
                Camera = camera
            },
            tx);
    }

    // Partial upsert — used by PATCH: COALESCE preserves existing value when
    // the incoming field is NULL (i.e. the client did not send it).
    private static async Task PatchTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int artworkId, string culture,
        string? title, string? description,
        string? historicalPeriod, string? support, string? camera)
    {
        await conn.ExecuteAsync("""
            INSERT INTO artwork_translations
                (artwork_id, language_code, title, description, historical_period, support, camera)
            VALUES
                (@ArtworkId, @Culture, @Title, @Description, @HistoricalPeriod, @Support, @Camera)
            ON CONFLICT (artwork_id, language_code) DO UPDATE SET
                title             = COALESCE(EXCLUDED.title,             artwork_translations.title),
                description       = COALESCE(EXCLUDED.description,       artwork_translations.description),
                historical_period = COALESCE(EXCLUDED.historical_period, artwork_translations.historical_period),
                support           = COALESCE(EXCLUDED.support,           artwork_translations.support),
                camera            = COALESCE(EXCLUDED.camera,            artwork_translations.camera);
            """,
            new
            {
                ArtworkId = artworkId,
                Culture = culture,
                Title = title,
                Description = description,
                HistoricalPeriod = historicalPeriod,
                Support = support,
                Camera = camera
            },
            tx);
    }
}

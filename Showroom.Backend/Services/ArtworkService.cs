// ArtworkService.cs
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

    private const string SelectFragment = """
        SELECT
            a.id                                                AS Id, 
            a.archive_id                                        AS ArchiveId, 
            a.name                                              AS Name, 
            a.year                                              AS Year, 
            a.dimensions                                        AS Dimensions, 
            a.image_url                                         AS ImageUrl,
            COALESCE(t.title,             en.title)             AS Title,
            COALESCE(t.description,       en.description)       AS Description,
            COALESCE(t.historical_period, en.historical_period) AS HistoricalPeriod,
            COALESCE(t.support,           en.support)           AS Support,
            COALESCE(t.camera,            en.camera)            AS Camera
        FROM artworks a
        LEFT JOIN artwork_translations t  ON t.artwork_id = a.id AND t.language_code = @Culture
        LEFT JOIN artwork_translations en ON en.artwork_id = a.id AND en.language_code = 'en'
        """;

    public async Task<IEnumerable<ArtworkDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            ORDER BY a.name
            """;
        return await conn.QueryAsync<ArtworkDto>(query, new { Culture = culture });
    }

    public async Task<ArtworkDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = SelectFragment + """
            
            WHERE a.id = @Id
            """;
        return await conn.QuerySingleOrDefaultAsync<ArtworkDto>(query, new { Id = id, Culture = culture });
    }

    public async Task<ArtworkDto> CreateAsync(CreateArtworkDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            const string query = """
                INSERT INTO artworks (archive_id, name, year, dimensions, image_url)
                VALUES (@ArchiveId, @Name, @Year, @Dimensions, @ImageUrl)
                RETURNING id
                """;

            int id = await conn.ExecuteScalarAsync<int>(query, dto, tx);

            await UpsertTranslationAsync(conn, tx, id, dto.Culture, dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<ArtworkDto?> UpdateAsync(int id, UpdateArtworkDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            const string query = """
                UPDATE artworks 
                SET archive_id = @ArchiveId, 
                    name = @Name, 
                    year = @Year, 
                    dimensions = @Dimensions, 
                    image_url = @ImageUrl
                WHERE id = @Id
                """;

            int rows = await conn.ExecuteAsync(query,
                new { Id = id, dto.ArchiveId, dto.Name, dto.Year, dto.Dimensions, dto.ImageUrl }, tx);

            if (rows == 0) return null;

            await UpsertTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<ArtworkDto?> PatchAsync(int id, PatchArtworkDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();
        try
        {
            var sets = new List<string>();
            var p = new DynamicParameters(new { Id = id });

            if (dto.ArchiveId != null) { sets.Add("archive_id = @ArchiveId"); p.Add("ArchiveId", dto.ArchiveId); }
            if (dto.Name != null) { sets.Add("name = @Name"); p.Add("Name", dto.Name); }
            if (dto.Year != null) { sets.Add("year = @Year"); p.Add("Year", dto.Year); }
            if (dto.Dimensions != null) { sets.Add("dimensions = @Dimensions"); p.Add("Dimensions", dto.Dimensions); }
            if (dto.ImageUrl != null) { sets.Add("image_url = @ImageUrl"); p.Add("ImageUrl", dto.ImageUrl); }

            if (sets.Count > 0)
            {
                string query = $"""
                    UPDATE artworks 
                    SET {string.Join(", ", sets)} 
                    WHERE id = @Id
                    """;
                await conn.ExecuteAsync(query, p, tx);
            }
            else
            {
                const string existsQuery = """
                    SELECT EXISTS(SELECT 1 FROM artworks WHERE id = @Id)
                    """;
                bool exists = await conn.ExecuteScalarAsync<bool>(existsQuery, new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            if (dto.Title != null || dto.Description != null || dto.HistoricalPeriod != null || dto.Support != null || dto.Camera != null)
            {
                await PatchTranslationAsync(conn, tx, id, culture, dto.Title, dto.Description, dto.HistoricalPeriod, dto.Support, dto.Camera);
            }

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            DELETE FROM artworks 
            WHERE id = @Id
            """;
        return await conn.ExecuteAsync(query, new { Id = id }) > 0;
    }

    public async Task UpsertTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int artworkId, string culture, string? title, string? description, string? historicalPeriod, string? support, string? camera)
    {
        const string query = """
            INSERT INTO artwork_translations (artwork_id, language_code, title, description, historical_period, support, camera)
            VALUES (@ArtworkId, @Culture, @Title, @Description, @HistoricalPeriod, @Support, @Camera)
            ON CONFLICT (artwork_id, language_code) DO UPDATE SET
                title             = EXCLUDED.title,
                description       = EXCLUDED.description,
                historical_period = EXCLUDED.historical_period,
                support           = EXCLUDED.support,
                camera            = EXCLUDED.camera
            """;
        await conn.ExecuteAsync(query, new { ArtworkId = artworkId, Culture = culture, Title = title, Description = description, HistoricalPeriod = historicalPeriod, Support = support, Camera = camera }, tx);
    }

    public async Task PatchTranslationAsync(NpgsqlConnection conn, NpgsqlTransaction tx, int artworkId, string culture, string? title, string? description, string? historicalPeriod, string? support, string? camera)
    {
        const string query = """
            INSERT INTO artwork_translations (artwork_id, language_code, title, description, historical_period, support, camera)
            VALUES (@ArtworkId, @Culture, @Title, @Description, @HistoricalPeriod, @Support, @Camera)
            ON CONFLICT (artwork_id, language_code) DO UPDATE SET
                title             = COALESCE(EXCLUDED.title,             artwork_translations.title),
                description       = COALESCE(EXCLUDED.description,       artwork_translations.description),
                historical_period = COALESCE(EXCLUDED.historical_period, artwork_translations.historical_period),
                support           = COALESCE(EXCLUDED.support,           artwork_translations.support),
                camera            = COALESCE(EXCLUDED.camera,            artwork_translations.camera)
            """;
        await conn.ExecuteAsync(query, new { ArtworkId = artworkId, Culture = culture, Title = title, Description = description, HistoricalPeriod = historicalPeriod, Support = support, Camera = camera }, tx);
    }
}
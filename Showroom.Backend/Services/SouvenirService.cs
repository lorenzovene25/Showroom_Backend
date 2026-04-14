using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

// ══════════════════════════════════════════════════════════════════
//  SOUVENIR SERVICE
// ══════════════════════════════════════════════════════════════════

public class SouvenirService : ISouvenirService
{
    private readonly string _connectionString;

    public SouvenirService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    private const string SelectFragment = """
     SELECT 
        s.id                                              AS Id, 
        s.archive_id                                      AS ArchiveId, 
        s.category_id                                     AS CategoryId, 
        c.slug                                            AS CategorySlug,
        s.price                                           AS Price, 
        s.in_stock                                        AS InStock, 
        s.quantity_available                              AS QuantityAvailable,
        s.image_url                                       AS ImageUrl, 
        s.specifications::text                            AS Specifications,
        COALESCE(t.name,                 en.name)                 AS Name,
        COALESCE(t.short_description,    en.short_description)    AS ShortDescription,
        COALESCE(t.full_description,     en.full_description)     AS FullDescription,
        COALESCE(t.specifications::text, en.specifications::text) AS TranslatedSpecifications
    FROM souvenirs s
    JOIN categories c ON c.id = s.category_id
    LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
    LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
    """;

    // GET ALL
    public async Task<IEnumerable<SouvenirDto>> GetAllAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<SouvenirDto>(
            SelectFragment + " ORDER BY s.id",
            new { Culture = culture });
    }

    // GET BY ID
    public async Task<SouvenirDto?> GetByIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<SouvenirDto>(
            SelectFragment + " WHERE s.id = @Id",
            new { Id = id, Culture = culture });
    }

    // GET BY CATEGORY
    public async Task<IEnumerable<SouvenirDto>> GetByCategoryAsync(int categoryId, string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<SouvenirDto>(
            SelectFragment + " WHERE s.category_id = @CategoryId ORDER BY s.id",
            new { CategoryId = categoryId, Culture = culture });
    }

    // GET IN STOCK ONLY
    public async Task<IEnumerable<SouvenirDto>> GetInStockAsync(string culture = "en")
    {
        using var conn = Conn();
        return await conn.QueryAsync<SouvenirDto>(
            SelectFragment + " WHERE s.in_stock = TRUE ORDER BY s.id",
            new { Culture = culture });
    }

    // POST
    public async Task<SouvenirDto> CreateAsync(CreateSouvenirDto dto)
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            // Aggiunto l'alias AS Id qui
            int id = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO souvenirs
                    (archive_id, category_id, price, in_stock, quantity_available, image_url, specifications)
                VALUES
                    (@ArchiveId, @CategoryId, @Price, @InStock, @QuantityAvailable,
                     @ImageUrl,  @Specifications::jsonb)
                RETURNING id AS Id; 
                """, dto, tx);

            await UpsertTranslationAsync(conn, tx, id, dto.Culture,
                dto.Name, dto.ShortDescription, dto.FullDescription, dto.TranslatedSpecifications);

            await tx.CommitAsync();
            return (await GetByIdAsync(id, dto.Culture))!;
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PUT
    public async Task<SouvenirDto?> UpdateAsync(int id, UpdateSouvenirDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            int rows = await conn.ExecuteAsync("""
                UPDATE souvenirs
                SET archive_id         = @ArchiveId,
                    category_id        = @CategoryId,
                    price              = @Price,
                    in_stock           = @InStock,
                    quantity_available = @QuantityAvailable,
                    image_url          = @ImageUrl,
                    specifications     = @Specifications::jsonb
                WHERE id = @Id;
                """, new
            {
                Id = id,
                dto.ArchiveId,
                dto.CategoryId,
                dto.Price,
                dto.InStock,
                dto.QuantityAvailable,
                dto.ImageUrl,
                dto.Specifications
            }, tx);

            if (rows == 0) { await tx.RollbackAsync(); return null; }

            await UpsertTranslationAsync(conn, tx, id, culture,
                dto.Name, dto.ShortDescription, dto.FullDescription, dto.TranslatedSpecifications);

            await tx.CommitAsync();
            return await GetByIdAsync(id, culture);
        }
        catch { await tx.RollbackAsync(); throw; }
    }

    // PATCH
    public async Task<SouvenirDto?> PatchAsync(int id, PatchSouvenirDto dto, string culture = "en")
    {
        using var conn = Conn();
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var sets = new List<string>();
            var p = new DynamicParameters(new { Id = id });

            if (dto.ArchiveId != null) { sets.Add("archive_id = @ArchiveId"); p.Add("ArchiveId", dto.ArchiveId); }
            if (dto.CategoryId != null) { sets.Add("category_id = @CategoryId"); p.Add("CategoryId", dto.CategoryId); }
            if (dto.Price != null) { sets.Add("price = @Price"); p.Add("Price", dto.Price); }
            if (dto.InStock != null) { sets.Add("in_stock = @InStock"); p.Add("InStock", dto.InStock); }
            if (dto.QuantityAvailable != null) { sets.Add("quantity_available = @QuantityAvailable"); p.Add("QuantityAvailable", dto.QuantityAvailable); }
            if (dto.ImageUrl != null) { sets.Add("image_url = @ImageUrl"); p.Add("ImageUrl", dto.ImageUrl); }
            if (dto.Specifications != null) { sets.Add("specifications = @Specifications::jsonb"); p.Add("Specifications", dto.Specifications); }

            if (sets.Count > 0)
            {
                int rows = await conn.ExecuteAsync(
                    $"UPDATE souvenirs SET {string.Join(", ", sets)} WHERE id = @Id", p, tx);
                if (rows == 0) { await tx.RollbackAsync(); return null; }
            }
            else
            {
                bool exists = await conn.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM souvenirs WHERE id = @Id)", new { Id = id }, tx);
                if (!exists) { await tx.RollbackAsync(); return null; }
            }

            bool hasTransFields = dto.Name != null || dto.ShortDescription != null ||
                                  dto.FullDescription != null || dto.TranslatedSpecifications != null;
            if (hasTransFields)
                await PatchTranslationAsync(conn, tx, id, culture,
                    dto.Name, dto.ShortDescription, dto.FullDescription, dto.TranslatedSpecifications);

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
            "DELETE FROM souvenirs WHERE id = @Id", new { Id = id }) > 0;
    }

    // ── PRIVATE HELPERS ──────────────────────────────────────────

    private static async Task UpsertTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int souvenirId, string culture,
        string? name, string? shortDesc, string? fullDesc, string? specs)
    {
        await conn.ExecuteAsync("""
            INSERT INTO souvenirs_translations
                (souvenir_id, language_code, name, short_description, full_description, specifications)
            VALUES
                (@SouvenirId, @Culture, @Name, @ShortDescription, @FullDescription, @Specifications::jsonb)
            ON CONFLICT (souvenir_id, language_code) DO UPDATE SET
                name              = EXCLUDED.name,
                short_description = EXCLUDED.short_description,
                full_description  = EXCLUDED.full_description,
                specifications    = EXCLUDED.specifications;
            """,
            new
            {
                SouvenirId = souvenirId,
                Culture = culture,
                Name = name,
                ShortDescription = shortDesc,
                FullDescription = fullDesc,
                Specifications = specs
            }, tx);
    }

    private static async Task PatchTranslationAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        int souvenirId, string culture,
        string? name, string? shortDesc, string? fullDesc, string? specs)
    {
        await conn.ExecuteAsync("""
            INSERT INTO souvenirs_translations
                (souvenir_id, language_code, name, short_description, full_description, specifications)
            VALUES
                (@SouvenirId, @Culture, @Name, @ShortDescription, @FullDescription, @Specifications::jsonb)
            ON CONFLICT (souvenir_id, language_code) DO UPDATE SET
                name              = COALESCE(EXCLUDED.name,              souvenirs_translations.name),
                short_description = COALESCE(EXCLUDED.short_description, souvenirs_translations.short_description),
                full_description  = COALESCE(EXCLUDED.full_description,  souvenirs_translations.full_description),
                specifications    = COALESCE(EXCLUDED.specifications,    souvenirs_translations.specifications);
            """,
            new
            {
                SouvenirId = souvenirId,
                Culture = culture,
                Name = name,
                ShortDescription = shortDesc,
                FullDescription = fullDesc,
                Specifications = specs
            }, tx);
    }
}
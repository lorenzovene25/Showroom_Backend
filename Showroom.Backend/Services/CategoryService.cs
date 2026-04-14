using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

public class CategoryService : ICategoryService
{
    private readonly string _connectionString;

    public CategoryService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                c.id          AS Id, 
                c.slug        AS Slug, 
                c.name        AS Name, 
                c.description AS Description
            FROM categories c
            ORDER BY c.name
            """;
        return await conn.QueryAsync<CategoryDto>(query);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                id          AS Id, 
                slug        AS Slug, 
                name        AS Name, 
                description AS Description 
            FROM categories 
            WHERE id = @Id
            """;
        return await conn.QuerySingleOrDefaultAsync<CategoryDto>(query, new { Id = id });
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                id          AS Id, 
                slug        AS Slug, 
                name        AS Name, 
                description AS Description 
            FROM categories 
            WHERE slug = @Slug
            """;
        return await conn.QuerySingleOrDefaultAsync<CategoryDto>(query, new { Slug = slug });
    }

    public async Task<IEnumerable<SouvenirDto>> GetSouvenirsByCategoryIdAsync(int id, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            SELECT 
                s.id                      AS Id,
                s.category_id             AS CategoryId,
                s.price                   AS Price,
                s.image_url               AS ImageUrl,
                s.in_stock                AS InStock,
                COALESCE(t.name, en.name) AS Name,
                COALESCE(t.short_description, en.short_description) AS ShortDescription,
                COALESCE(t.full_description, en.full_description) AS FullDescription
            FROM souvenirs s
            LEFT JOIN souvenirs_translations t  ON t.souvenir_id = s.id AND t.language_code = @Culture
            LEFT JOIN souvenirs_translations en ON en.souvenir_id = s.id AND en.language_code = 'en'
            WHERE s.category_id = @Id
            ORDER BY Name
            """;
        return await conn.QueryAsync<SouvenirDto>(query, new { Id = id, Culture = culture });
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        using var conn = Conn();
        const string query = """
            INSERT INTO categories (slug, name, description)
            VALUES (@Slug, @Name, @Description)
            RETURNING id
            """;
        int id = await conn.ExecuteScalarAsync<int>(query, dto);
        return (await GetByIdAsync(id))!;
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto, string culture = "en")
    {
        using var conn = Conn();
        const string query = """
            UPDATE categories 
            SET slug = @Slug, 
                name = @Name, 
                description = @Description 
            WHERE id = @Id
            """;
        int rows = await conn.ExecuteAsync(query, new { Id = id, dto.Slug, dto.Name, dto.Description });
        return rows == 0 ? null : await GetByIdAsync(id);
    }

    public async Task<CategoryDto?> PatchAsync(int id, PatchCategoryDto dto, string culture = "en")
    {
        using var conn = Conn();
        var sets = new List<string>();
        var p = new DynamicParameters(new { Id = id });

        if (dto.Slug != null) { sets.Add("slug = @Slug"); p.Add("Slug", dto.Slug); }
        if (dto.Name != null) { sets.Add("name = @Name"); p.Add("Name", dto.Name); }
        if (dto.Description != null) { sets.Add("description = @Description"); p.Add("Description", dto.Description); }

        if (sets.Count == 0) return await GetByIdAsync(id);

        string query = $"""
            UPDATE categories 
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
            DELETE FROM categories 
            WHERE id = @Id
            """;
        return await conn.ExecuteAsync(query, new { Id = id }) > 0;
    }
}
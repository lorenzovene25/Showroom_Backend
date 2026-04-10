using Dapper;
using Npgsql;
using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public class CategoryService : ICategoryService
{
    private readonly string _connectionString;

    public CategoryService(IConfiguration configuration)
        => _connectionString = configuration.GetConnectionString("db")!;

    private NpgsqlConnection Conn() => new(_connectionString);

    // GET ALL
    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        using var conn = Conn();
        return await conn.QueryAsync<CategoryDto>(
            "SELECT id AS Id, slug AS Slug, name AS Name, description AS Description FROM categories ORDER BY name");
    }

    // GET BY ID
    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<CategoryDto>(
            "SELECT id AS Id, slug AS Slug, name AS Name, description AS Description FROM categories WHERE id = @Id", new { Id = id });
    }

    // GET BY SLUG
    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        using var conn = Conn();
        return await conn.QuerySingleOrDefaultAsync<CategoryDto>(
            "SELECT id AS Id, slug AS Slug, name AS Name, description AS Description FROM categories WHERE slug = @Slug", new { Slug = slug });
    }

    // POST
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        using var conn = Conn();
        int id = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO categories (slug, name, description)
            VALUES (@Slug, @Name, @Description)
            RETURNING id AS Id;
            """, dto);
        return (await GetByIdAsync(id))!;
    }

    // PUT
    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        using var conn = Conn();
        int rows = await conn.ExecuteAsync("""
            UPDATE categories
            SET slug = @Slug, name = @Name, description = @Description
            WHERE id = @Id;
            """, new { Id = id, dto.Slug, dto.Name, dto.Description });
        return rows == 0 ? null : await GetByIdAsync(id);
    }

    // PATCH
    public async Task<CategoryDto?> PatchAsync(int id, PatchCategoryDto dto)
    {
        using var conn = Conn();
        var sets = new List<string>();
        var p = new DynamicParameters(new { Id = id });

        if (dto.Slug != null) { sets.Add("slug = @Slug"); p.Add("Slug", dto.Slug); }
        if (dto.Name != null) { sets.Add("name = @Name"); p.Add("Name", dto.Name); }
        if (dto.Description != null) { sets.Add("description = @Description"); p.Add("Description", dto.Description); }

        if (sets.Count == 0) return await GetByIdAsync(id);

        int rows = await conn.ExecuteAsync(
            $"UPDATE categories SET {string.Join(", ", sets)} WHERE id = @Id", p);
        return rows == 0 ? null : await GetByIdAsync(id);
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = Conn();
        return await conn.ExecuteAsync(
            "DELETE FROM categories WHERE id = @Id", new { Id = id }) > 0;
    }
}
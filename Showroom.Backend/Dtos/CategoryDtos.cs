namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  CATEGORY
// ══════════════════════════════════════════════════════════════════

public class CategoryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateCategoryDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PatchCategoryDto
{
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
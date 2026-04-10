namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  CATEGORY
// ══════════════════════════════════════════════════════════════════

public record CategoryDto(
    int Id, string Slug, string Name, string? Description);

public record CreateCategoryDto(
    string Slug, string Name, string? Description = null);

public record UpdateCategoryDto(
    string Slug, string Name, string? Description = null);

public record PatchCategoryDto(
    string? Slug = null, string? Name = null, string? Description = null);

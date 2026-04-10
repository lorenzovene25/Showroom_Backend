namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  EXHIBITION
// ══════════════════════════════════════════════════════════════════

public record ExhibitionDto(
    int Id, string Name, string Location, string? MapsUrl,
    string Status, DateOnly StartDate, DateOnly EndDate, string? ImageUrl,
    // from exhibition_translations (culture-aware)
    string? Title, string? Description);

public record CreateExhibitionDto(
    string Name, string Location, string Status,
    DateOnly StartDate, DateOnly EndDate,
    string? MapsUrl = null, string? ImageUrl = null,
    string Culture = "en",
    string? Title = null, string? Description = null);

public record UpdateExhibitionDto(
    string Name, string Location, string Status,
    DateOnly StartDate, DateOnly EndDate,
    string? MapsUrl = null, string? ImageUrl = null,
    string? Title = null, string? Description = null);

public record PatchExhibitionDto(
    string? Name = null, string? Location = null, string? MapsUrl = null,
    string? Status = null, DateOnly? StartDate = null, DateOnly? EndDate = null,
    string? ImageUrl = null, string? Title = null, string? Description = null);
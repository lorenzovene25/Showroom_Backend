namespace Showroom.Backend.Dtos;

public record ArtworkDto(
    int Id, string? ArchiveId, string Name, int? Year,
    string? Dimensions, string? ImageUrl,
    // from artwork_translations (culture-aware)
    string? Title, string? Description,
    string? HistoricalPeriod, string? Support, string? Camera);

public record CreateArtworkDto(
    string Name,
    string? ArchiveId = null, int? Year = null,
    string? Dimensions = null, string? ImageUrl = null,
    string Culture = "en",
    string? Title = null, string? Description = null,
    string? HistoricalPeriod = null, string? Support = null, string? Camera = null);

public record UpdateArtworkDto(
    string Name,
    string? ArchiveId = null, int? Year = null,
    string? Dimensions = null, string? ImageUrl = null,
    string? Title = null, string? Description = null,
    string? HistoricalPeriod = null, string? Support = null, string? Camera = null);

public record PatchArtworkDto(
    string? Name = null, string? ArchiveId = null, int? Year = null,
    string? Dimensions = null, string? ImageUrl = null,
    string? Title = null, string? Description = null,
    string? HistoricalPeriod = null, string? Support = null, string? Camera = null);
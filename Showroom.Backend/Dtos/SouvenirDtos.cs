namespace Showroom.Backend.Dtos;

// ══════════════════════════════════════════════════════════════════
//  SOUVENIR
// ══════════════════════════════════════════════════════════════════

public record SouvenirDto(
    int Id, string? ArchiveId, int CategoryId, string? CategorySlug,
    decimal Price, bool InStock, int QuantityAvailable,
    string? ImageUrl, string? Specifications,
    // from souvenirs_translations (culture-aware)
    string? Name, string? ShortDescription, string? FullDescription,
    string? TranslatedSpecifications);

public record CreateSouvenirDto(
    int CategoryId, decimal Price,
    string? ArchiveId = null, bool InStock = true, int QuantityAvailable = 0,
    string? ImageUrl = null, string? Specifications = null,
    string Culture = "en",
    string? Name = null, string? ShortDescription = null,
    string? FullDescription = null, string? TranslatedSpecifications = null);

public record UpdateSouvenirDto(
    int CategoryId, decimal Price,
    string? ArchiveId = null, bool InStock = true, int QuantityAvailable = 0,
    string? ImageUrl = null, string? Specifications = null,
    string? Name = null, string? ShortDescription = null,
    string? FullDescription = null, string? TranslatedSpecifications = null);

public record PatchSouvenirDto(
    int? CategoryId = null, decimal? Price = null,
    string? ArchiveId = null, bool? InStock = null, int? QuantityAvailable = null,
    string? ImageUrl = null, string? Specifications = null,
    string? Name = null, string? ShortDescription = null,
    string? FullDescription = null, string? TranslatedSpecifications = null);

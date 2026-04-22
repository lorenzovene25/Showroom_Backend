namespace Showroom.Backend.Dtos;

/// <summary>
/// DTO per le immagini di una galleria
/// </summary>
public record ArtistGalleryDto(
    string ArtistName,
    string? Preview,
    List<string> ArtworkImages,
    List<string> SouvenirImages
);
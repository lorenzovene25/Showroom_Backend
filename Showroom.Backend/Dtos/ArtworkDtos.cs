namespace Showroom.Backend.Dtos;

public class ArtworkDto
{
    public int Id { get; set; }
    public string? ArchiveId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }

    // from artwork_translations (culture-aware)
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? HistoricalPeriod { get; set; }
    public string? Support { get; set; }
    public string? Camera { get; set; }
}

public class CreateArtworkDto
{
    public string Name { get; set; } = string.Empty;
    public string? ArchiveId { get; set; }
    public int? Year { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public string Culture { get; set; } = "en";
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? HistoricalPeriod { get; set; }
    public string? Support { get; set; }
    public string? Camera { get; set; }
}

public class UpdateArtworkDto
{
    public string Name { get; set; } = string.Empty;
    public string? ArchiveId { get; set; }
    public int? Year { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? HistoricalPeriod { get; set; }
    public string? Support { get; set; }
    public string? Camera { get; set; }
}

public class PatchArtworkDto
{
    public string? Name { get; set; }
    public string? ArchiveId { get; set; }
    public int? Year { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? HistoricalPeriod { get; set; }
    public string? Support { get; set; }
    public string? Camera { get; set; }
}
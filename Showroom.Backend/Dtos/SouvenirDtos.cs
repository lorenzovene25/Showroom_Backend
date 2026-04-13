namespace Showroom.Backend.Dtos;

public class SouvenirDto
{
    public int Id { get; set; }
    public string? ArchiveId { get; set; }
    public int CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public int QuantityAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; }
        public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? TranslatedSpecifications { get; set; }
}

public class CreateSouvenirDto
{
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public string? ArchiveId { get; set; }
    public bool InStock { get; set; } = true;
    public int QuantityAvailable { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; }
    public string Culture { get; set; } = "en";
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? TranslatedSpecifications { get; set; }
}

public class UpdateSouvenirDto
{
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public string? ArchiveId { get; set; }
    public bool InStock { get; set; } = true;
    public int QuantityAvailable { get; set; } = 0;
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; }
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? TranslatedSpecifications { get; set; }
}

public class PatchSouvenirDto
{
    public int? CategoryId { get; set; }
    public decimal? Price { get; set; }
    public string? ArchiveId { get; set; }
    public bool? InStock { get; set; }
    public int? QuantityAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; }
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? TranslatedSpecifications { get; set; }
}
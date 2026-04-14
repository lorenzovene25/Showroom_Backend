using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Endpoints;

public static class ArtworkEndpoints
{
    public static void MapArtworkEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/artworks")
                         .WithTags("Artworks")
                         .RequireRateLimiting("RateLimit");

        // GET /artworks?culture=en
        group.MapGet("", GetArtworksAsync)
            .WithName("GetArtworks")
            .WithSummary("Restituisce tutte le opere");

        // GET /artworks/{id}?culture=en
        group.MapGet("/{id:int}", GetArtworkByIdAsync)
            .WithName("GetArtworkById")
            .WithSummary("Restituisce un'opera per ID");
    }

    public static async Task<Results<Ok<IEnumerable<ArtworkDto>>, NotFound>> GetArtworksAsync(IArtworkService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("ArtworkEndpoints");
        logger.LogInformation("Fetching all artworks - Culture: {Culture}", culture);

        var items = await service.GetAllAsync(culture);

        if (items is null || !items.Any())
        {
            logger.LogWarning("No artworks found for culture: {Culture}", culture);
            return TypedResults.NotFound();
        }

        var result = items.Select(artwork => new ArtworkDto
        {
            Id = artwork.Id,
            ArchiveId = artwork.ArchiveId,
            Name = artwork.Name,
            Year = artwork.Year,
            Dimensions = artwork.Dimensions,
            ImageUrl = artwork.ImageUrl,
            Title = artwork.Title,
            Description = artwork.Description,
            HistoricalPeriod = artwork.HistoricalPeriod,
            Support = artwork.Support,
            Camera = artwork.Camera
        });

        logger.LogInformation("Successfully fetched {Count} artworks - Culture: {Culture}", result.Count(), culture);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<ArtworkDto>, NotFound>> GetArtworkByIdAsync(int id, IArtworkService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("ArtworkEndpoints");
        logger.LogInformation("Fetching artwork by ID: {ArtworkId} - Culture: {Culture}", id, culture);

        var item = await service.GetByIdAsync(id, culture);

        if (item is null)
        {
            logger.LogWarning("Artwork not found - ID: {ArtworkId}", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Artwork found - ID: {ArtworkId}", id);
        return TypedResults.Ok(item);
    }
}
using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

public static class ArtworkEndpoints
{
    public static void MapArtworkEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/artworks")
                         .WithTags("Artworks")
                         .RequireRateLimiting("BaseRule");

        // GET /artworks?culture=en
        group.MapGet("", GetArtworksAsync)
            .WithName("GetArtworks")
            .WithSummary("Restituisce tutte le opere");

        // GET /artworks/{id}?culture=en
        group.MapGet("/{id:int}", GetArtworkByIdAsync)
            .WithName("GetArtworkById")
            .WithSummary("Restituisce un'opera per ID");
    }

    public static async Task<Results<Ok<IEnumerable<ArtworkDto>>, NotFound>> GetArtworksAsync(IArtworkService service, HttpContext context, string culture = "en")
    {

        var items = await service.GetAllAsync(culture);

        if (items is null || !items.Any())
            return TypedResults.NotFound();

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

        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<ArtworkDto>, NotFound>> GetArtworkByIdAsync(int id, IArtworkService service, HttpContext context, string culture = "en")
    {
        var item = await service.GetByIdAsync(id, culture);

        if (item is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(item);
    }
}
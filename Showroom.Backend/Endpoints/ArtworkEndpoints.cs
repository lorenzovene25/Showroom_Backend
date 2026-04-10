using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;


// ══════════════════════════════════════════════════════════════════
//  OPERE  (Artworks)
// ══════════════════════════════════════════════════════════════════
public static class ArtworkEndpoints
{


    public static void MapArtworkEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /artworks?culture=en
        app.MapGet("/artworks", async (IArtworkService svc, string culture = "en") =>
        {
            var result = await svc.GetAllAsync(culture);
            return Results.Ok(result);
        })
        .WithName("GetArtworks")
        .WithTags("Artworks")
        .WithSummary("Restituisce tutte le opere")
        .Produces<IEnumerable<ArtworkDto>>();

        // GET /artworks/{id}?culture=en
        app.MapGet("/artworks/{id:int}", async (int id, IArtworkService svc, string culture = "en") =>
        {
            var result = await svc.GetByIdAsync(id, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetArtworkById")
        .WithTags("Artworks")
        .WithSummary("Restituisce un'opera per ID")
        .Produces<ArtworkDto>()
        .ProducesProblem(404);
    }
}
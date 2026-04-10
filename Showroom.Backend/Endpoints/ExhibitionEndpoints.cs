using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

// ══════════════════════════════════════════════════════════════════
//  MOSTRE  (Exhibitions)
// ══════════════════════════════════════════════════════════════════
public static class ExhibitionEndpoints
{

    public static void MapExhibitionEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /mostre?culture=en
        app.MapGet("/mostre", async (IExhibitionService svc, string culture = "en") =>
        {
            var result = await svc.GetAllAsync(culture);
            return Results.Ok(result);
        })
        .WithName("GetMostre")
        .WithTags("Mostre")
        .WithSummary("Restituisce tutte le mostre")
        .Produces<IEnumerable<ExhibitionDto>>();

        // GET /mostre/{id}?culture=en
        app.MapGet("/mostre/{id:int}", async (int id, IExhibitionService svc, string culture = "en") =>
        {
            var result = await svc.GetByIdAsync(id, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetMostraById")
        .WithTags("Mostre")
        .WithSummary("Restituisce una mostra per ID")
        .Produces<ExhibitionDto>()
        .ProducesProblem(404);

        // POST /mostre
        app.MapPost("/mostre", async (CreateExhibitionDto dto, IExhibitionService svc) =>
        {
            var created = await svc.CreateAsync(dto);
            return Results.Created($"/mostre/{created.Id}", created);
        })
        .WithName("CreateMostra")
        .WithTags("Mostre")
        .WithSummary("Crea una nuova mostra")
        .Produces<ExhibitionDto>(201);

        // PATCH /mostre/{id}?culture=en
        app.MapPatch("/mostre/{id:int}", async (int id, PatchExhibitionDto dto, IExhibitionService svc, string culture = "en") =>
        {
            var result = await svc.PatchAsync(id, dto, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("PatchMostra")
        .WithTags("Mostre")
        .WithSummary("Aggiorna parzialmente una mostra")
        .Produces<ExhibitionDto>()
        .ProducesProblem(404);
    }
}
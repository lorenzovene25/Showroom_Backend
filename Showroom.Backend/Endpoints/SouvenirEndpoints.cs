using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;


// ══════════════════════════════════════════════════════════════════
//  SOUVENIR
// ══════════════════════════════════════════════════════════════════
public static class SouvenirEndpoints
{

    public static void MapSouvenirEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /souvenirs?culture=en
        app.MapGet("/souvenirs", async (ISouvenirService svc, string culture = "en") =>
        {
            var result = await svc.GetAllAsync(culture);
            return Results.Ok(result);
        })
        .WithName("GetSouvenirs")
        .WithTags("Souvenirs")
        .WithSummary("Restituisce tutti i souvenir")
        .Produces<IEnumerable<SouvenirDto>>();

        // GET /souvenirs/{id}?culture=en
        app.MapGet("/souvenirs/{id:int}", async (int id, ISouvenirService svc, string culture = "en") =>
        {
            var result = await svc.GetByIdAsync(id, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetSouvenirById")
        .WithTags("Souvenirs")
        .WithSummary("Restituisce un souvenir per ID")
        .Produces<SouvenirDto>()
        .ProducesProblem(404);

        // POST /souvenirs
        app.MapPost("/souvenirs", async (CreateSouvenirDto dto, ISouvenirService svc) =>
        {
            var created = await svc.CreateAsync(dto);
            return Results.Created($"/souvenirs/{created.Id}", created);
        })
        .WithName("CreateSouvenir")
        .WithTags("Souvenirs")
        .WithSummary("Crea un nuovo souvenir")
        .Produces<SouvenirDto>(201);

        // PATCH /souvenirs/{id}?culture=en
        app.MapPatch("/souvenirs/{id:int}", async (int id, PatchSouvenirDto dto, ISouvenirService svc, string culture = "en") =>
        {
            var result = await svc.PatchAsync(id, dto, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("PatchSouvenir")
        .WithTags("Souvenirs")
        .WithSummary("Aggiorna parzialmente un souvenir")
        .Produces<SouvenirDto>()
        .ProducesProblem(404);

        // DELETE /souvenirs/{id}
        app.MapDelete("/souvenirs/{id:int}", async (int id, ISouvenirService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteSouvenir")
        .WithTags("Souvenirs")
        .WithSummary("Elimina un souvenir")
        .Produces(204)
        .ProducesProblem(404);
    }
}

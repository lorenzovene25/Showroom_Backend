using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;


// ══════════════════════════════════════════════════════════════════
//  TICKETS
// ══════════════════════════════════════════════════════════════════
public static class TicketEndpoints
{
public static void MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        // DELETE /tickets/{id}
        app.MapDelete("/tickets/{id:int}", async (int id, ITicketTierService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteTicket")
        .WithTags("Tickets")
        .WithSummary("Cancella (annulla) un ticket")
        .Produces(204)
        .ProducesProblem(404);

        // PATCH /tickets/{id}
        // I ticket non hanno campi modificabili post-emissione secondo il dominio,
        // ma l'endpoint è esposto per eventuali estensioni future (es. cambio data).
        // Ritorna 200 con il ticket invariato se non ci sono campi da aggiornare.
        app.MapPatch("/tickets/{id:int}", async (int id, ITicketTierService svc, string culture = "en") =>
        {
            var result = await svc.GetByIdAsync(id, culture);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("PatchTicket")
        .WithTags("Tickets")
        .WithSummary("Placeholder PATCH ticket (i ticket sono immutabili dopo l'emissione)")
        .Produces<TicketDto>()
        .ProducesProblem(404);
    }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

public static class SouvenirEndpoints
{
    public static void MapSouvenirEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/souvenirs")
                 .WithTags("Souvenirs")
                 .RequireRateLimiting("RateLimit");

        // GET /souvenirs?culture=en
        group.MapGet("", GetSouvenirsAsync)
        .WithName("GetSouvenirs")
        .WithSummary("Restituisce tutti i souvenir");

        // GET /souvenirs/{id}?culture=en
        group.MapGet("{id:int}", GetSouvenirsByIdAsync)
        .WithName("GetSouvenirById")
        .WithSummary("Restituisce un souvenir per ID");
    }

    public static async Task<Results<Ok<IEnumerable<SouvenirDto>>, NotFound>> GetSouvenirsAsync(ISouvenirService service, HttpContext context, string culture = "en")
    {
        var result = await service.GetAllAsync(culture);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<SouvenirDto>, NotFound>> GetSouvenirsByIdAsync(int id, ISouvenirService service, HttpContext context, string culture = "en")
    {
        var result = await service.GetByIdAsync(id, culture);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
}

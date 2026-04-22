using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;

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

    public static async Task<Results<Ok<IEnumerable<SouvenirDto>>, NotFound>> GetSouvenirsAsync(ISouvenirService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("SouvenirEndpoints");
        logger.LogInformation("Fetching all souvenirs - Culture: {Culture}", culture);

        var result = await service.GetAllAsync(culture);

        logger.LogInformation("Successfully fetched {Count} souvenirs - Culture: {Culture}", result?.Count() ?? 0, culture);
        return TypedResults.Ok(result);
    }

    public static async Task<Results<Ok<SouvenirDto>, NotFound>> GetSouvenirsByIdAsync(int id, ISouvenirService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("SouvenirEndpoints");
        logger.LogInformation("Fetching souvenir by ID: {SouvenirId} - Culture: {Culture}", id, culture);

        var result = await service.GetByIdAsync(id, culture);

        if (result is null)
        {
            logger.LogWarning("Souvenir not found - ID: {SouvenirId}", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Souvenir found - ID: {SouvenirId}", id);
        return TypedResults.Ok(result);
    }
}

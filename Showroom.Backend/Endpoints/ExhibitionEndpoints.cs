using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;
using Showroom.Models;
using System.Text.RegularExpressions;

namespace PW.WebApi.Endpoints
{
    public static class ExhibitionsEndpoints
    {
        public static void MapExhibitionEndpoints(this IEndpointRouteBuilder route)
        {
            var group = route.MapGroup("/api/exhibitions")
                             .WithTags("Exhibitions")
                             .RequireRateLimiting("RateLimit");

            group.MapGet("", GetExhibitionsAsync)
                 .WithName("GetExhibitionsList")
                 .WithDescription("Get list of exhibitions");

            group.MapGet("{id:int}", GetExhibitionByIdAsync)
                 .WithName("GetExhibitionById")
                 .WithDescription("Get detailed exhibition information");

            group.MapGet("{id:int}/timeslots", GetExhibitionTimeSlotsByIdAsync)
                 .WithName("GetExhibitionTimeSlotsById")
                 .WithDescription("Get exhibition time slots by ID");

            group.MapGet("{id:int}/artworks", GetExhibitionArtworksByIdAsync)
                 .WithName("GetExhibitionArtworksById")
                 .WithDescription("Get exhibition artworks by ID");

            group.MapGet("tiers", GetTicketTiers)
                 .WithName("GetTicketTiers")
                 .WithDescription("Get ticket tiers for exhibitions");
        }

        private static async Task<Results<Ok<IEnumerable<TicketTierDto>>, NotFound>> GetTicketTiers(ITicketService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("ExhibitionEndpoints");
            logger.LogInformation("Fetching ticket tiers - Culture: {Culture}", culture);

            var items = await service.GetAllTicketTiersAsync(culture);

            if (items is null || !items.Any())
            {
                logger.LogWarning("No ticket tiers found - Culture: {Culture}", culture);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} ticket tiers - Culture: {Culture}", items.Count(), culture);
            return TypedResults.Ok(items);
        }

        private static async Task<Results<Ok<IEnumerable<ArtworkDto>>, NotFound>> GetExhibitionArtworksByIdAsync(int id, IExhibitionService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("ExhibitionEndpoints");
            logger.LogInformation("Fetching artworks for exhibition ID: {ExhibitionId} - Culture: {Culture}", id, culture);

            var items = await service.GetAllArtworksAsync(id, culture);

            if (items is null || !items.Any())
            {
                logger.LogWarning("No artworks found for exhibition ID: {ExhibitionId}", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} artworks for exhibition ID: {ExhibitionId}", items.Count(), id);
            return TypedResults.Ok(items);
        }

        private static async Task<Results<Ok<IEnumerable<ExhibitionTimeSlotDto>>, NotFound>> GetExhibitionTimeSlotsByIdAsync(int id, IExhibitionService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("ExhibitionEndpoints");
            logger.LogInformation("Fetching time slots for exhibition ID: {ExhibitionId} - Culture: {Culture}", id, culture);

            var items = await service.GetAllTimeSlotsAsync(id, culture);

            if (items is null || !items.Any())
            {
                logger.LogWarning("No time slots found for exhibition ID: {ExhibitionId}", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} time slots for exhibition ID: {ExhibitionId}", items.Count(), id);
            return TypedResults.Ok(items);
        }

        public static async Task<Results<Ok<IEnumerable<ExhibitionDto>>, NotFound>> GetExhibitionsAsync(IExhibitionService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("ExhibitionEndpoints");
            logger.LogInformation("Fetching all exhibitions - Culture: {Culture}", culture);

            var items = await service.GetAllAsync(culture);

            if (items is null || !items.Any())
            {
                logger.LogWarning("No exhibitions found for culture: {Culture}", culture);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} exhibitions - Culture: {Culture}", items.Count(), culture);
            return TypedResults.Ok(items);
        }

        public static async Task<Results<Ok<ExhibitionDto>, NotFound>> GetExhibitionByIdAsync(int id, IExhibitionService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("ExhibitionEndpoints");
            logger.LogInformation("Fetching exhibition by ID: {ExhibitionId} - Culture: {Culture}", id, culture);

            var item = await service.GetByIdAsync(id, culture);

            if (item is null)
            {
                logger.LogWarning("Exhibition not found - ID: {ExhibitionId}", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Exhibition found - ID: {ExhibitionId}", id);
            return TypedResults.Ok(item);
        }
    }
}

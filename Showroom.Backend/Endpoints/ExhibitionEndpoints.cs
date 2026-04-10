using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;
using Showroom.Models;

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
        }

        private static async Task<Results<Ok<IEnumerable<ArtworkDto>>, NotFound>> GetExhibitionArtworksByIdAsync(int id, IExhibitionService service, HttpContext context, string culture = "en")
        {
            var items = await service.GetAllArtworksAsync(id, culture);

            if (items is null || !items.Any())
                return TypedResults.NotFound();

            return TypedResults.Ok(items);
        }

        private static async Task<Results<Ok<IEnumerable<ExhibitionTimeSlotDto>>, NotFound>> GetExhibitionTimeSlotsByIdAsync(int id, IExhibitionService service, HttpContext context, string culture = "en")
        {
            var items = await service.GetAllTimeSlotsAsync(id, culture);

            if (items is null || !items.Any())
                return TypedResults.NotFound();

            return TypedResults.Ok(items);
        }

        public static async Task<Results<Ok<IEnumerable<ExhibitionDto>>, NotFound>> GetExhibitionsAsync(IExhibitionService service, HttpContext context, string culture = "en")
        {
            var items = await service.GetAllAsync(culture);

            if (items is null || !items.Any())
                return TypedResults.NotFound();

            return TypedResults.Ok(items);
        }

        public static async Task<Results<Ok<ExhibitionDto>, NotFound>> GetExhibitionByIdAsync(int id, IExhibitionService service, HttpContext context, string culture = "en")
        {
            var item = await service.GetByIdAsync(id, culture);

            if (item is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(item);
        }
    }
}

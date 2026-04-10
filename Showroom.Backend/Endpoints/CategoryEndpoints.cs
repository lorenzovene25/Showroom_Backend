using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder route)
        {
            var group = route.MapGroup("/api/categories")
                             .WithTags("Categories")
                             .RequireRateLimiting("RateLimit");

            // GET /categories?culture=en
            group.MapGet("", GetCategoriesAsync)
                .WithName("GetCategories")
                .WithTags("Categories")
                .WithSummary("Restituisce tutte le categorie");

            // GET /categories/{id}?culture=en
            group.MapGet("{id:int}", GetCategoryByIdAsync)
                .WithName("GetCategoryById")
                .WithTags("Categories")
                .WithSummary("Restituisce una categoria per ID");

            // GET /categories/{id}/souvenirs?culture=en
            group.MapGet("{id:int}/souvenirs", GetCategorySouvenirsAsync)
                .WithName("GetCategorySouvenirs")
                .WithTags("Categories")
                .WithSummary("Restituisce i souvenir di una categoria per ID");
        }

        private static async Task<Results<Ok<IEnumerable<SouvenirDto>>, NotFound>> GetCategorySouvenirsAsync(int id, ICategoryService service, HttpContext context, string culture = "en")
        {
            var items = await service.GetSouvenirsByCategoryIdAsync(id, culture);

            if (items is null || !items.Any())
                return TypedResults.NotFound();

            return TypedResults.Ok(items);
        }

        private static async Task<Results<Ok<CategoryDto>, NotFound>> GetCategoryByIdAsync(int id, ICategoryService service, HttpContext context, string culture = "en")
        {
            var item = await service.GetByIdAsync(id, culture);

            if (item is null)
                return TypedResults.NotFound(); 
            return TypedResults.Ok(item);

        }

        private static async Task<Results<Ok<IEnumerable<CategoryDto>>, NotFound>> GetCategoriesAsync(ICategoryService service, HttpContext context, string culture = "en")
        {
            var items = await service.GetAllAsync(culture);

            if (items is null || !items.Any())
                return TypedResults.NotFound();

            return TypedResults.Ok(items);
        }
    }
}

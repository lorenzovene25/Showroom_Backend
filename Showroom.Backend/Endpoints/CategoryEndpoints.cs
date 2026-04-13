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

        private static async Task<Results<Ok<IEnumerable<SouvenirDto>>, NotFound>> GetCategorySouvenirsAsync(int id, ICategoryService service, ILoggerFactory loggerFactory, HttpContext context, string culture = "en")
        {
            var logger = loggerFactory.CreateLogger("CategoryEndpoints");
            logger.LogInformation("Fetching souvenirs for category ID: {CategoryId} - Culture: {Culture}", id, culture);

            var items = await service.GetSouvenirsByCategoryIdAsync(id, culture);

            if (items is null || !items.Any())
            {
                logger.LogWarning("No souvenirs found for category ID: {CategoryId}", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} souvenirs for category ID: {CategoryId}", items.Count(), id);
            return TypedResults.Ok(items);
        }

        private static async Task<Results<Ok<CategoryDto>, NotFound>> GetCategoryByIdAsync(int id, ICategoryService service, ILoggerFactory loggerFactory, HttpContext context)
        {
            var logger = loggerFactory.CreateLogger("CategoryEndpoints");
            logger.LogInformation("Fetching category by ID: {CategoryId}", id);

            var item = await service.GetByIdAsync(id);

            if (item is null)
            {
                logger.LogWarning("Category not found - ID: {CategoryId}", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Category found - ID: {CategoryId}", id);
            return TypedResults.Ok(item);
        }

        private static async Task<Results<Ok<IEnumerable<CategoryDto>>, NotFound>> GetCategoriesAsync(ICategoryService service, ILoggerFactory loggerFactory, HttpContext context)
        {
            var logger = loggerFactory.CreateLogger("CategoryEndpoints");
            logger.LogInformation("Fetching all categories");

            var items = await service.GetAllAsync();

            if (items is null || !items.Any())
            {
                logger.LogWarning("No categories found");
                return TypedResults.NotFound();
            }

            logger.LogInformation("Successfully fetched {Count} categories", items.Count());
            return TypedResults.Ok(items);
        }
    }
}

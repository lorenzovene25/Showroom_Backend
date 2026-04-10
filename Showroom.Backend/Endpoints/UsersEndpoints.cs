using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/users")
                             .WithTags("Users")
                             .RequireRateLimiting("RateLimit");

        // GET /users/email/{email}
        group.MapGet("email/{email}", GetUserByEmail)
        .WithName("GetUserByEmail")
        .WithSummary("Restituisce un utente per email");

        // GET /users/{id}
        group.MapGet("{id:int}", GetUserById)
        .WithName("GetUserById")
        .WithSummary("Restituisce un utente per ID");

        group.MapGet("{userId:int}/cart", GetUserCart)
        .WithName("GetUserCart")
        .WithSummary("Restituisce il carrello di un utente per ID");

        group.MapGet("{userId:int}/tickets", GetUserTickets)
        .WithName("GetUserTickets")
        .WithSummary("Restituisce i biglietti di un utente per ID");

        group.MapGet("{userId:int}/orders", GetUserOrders)
        .WithName("GetUserOrders")
        .WithSummary("Restituisce gli ordini di un utente per ID");

        group.MapGet("{userId:int}/orders/{orderId:int}", GetUserOrderById)
        .WithName("GetUserOrderById")
        .WithSummary("Restituisce un ordine di un utente per ID");
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetUserByEmail(string email, IUserService service)
    {
        var result = await service.GetByEmailAsync(email);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }
    private static async Task<Results<Ok<UserDto>, NotFound>> GetUserById(int id, IUserService service)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IEnumerable<TicketDto>>, NotFound>> GetUserTickets(int userId, IUserService service, string culture = "en")
    {
        var result = await service.GetTicketsAsync(userId, culture);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IEnumerable<OrderDto>>, NotFound>> GetUserOrders(int userId, IUserService service, string culture = "en")
    {
        var result = await service.GetOrdersAsync(userId, culture);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OrderDto>, NotFound>> GetUserOrderById(int userId, int orderId, IUserService service, string culture = "en")
    {
        var result = await service.GetOrderByIdAsync(userId, orderId, culture);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CartDto>, NotFound>> GetUserCart(int userId, IUserService service, string culture = "en")
    {
        var result = await service.GetCartAsync(userId, culture);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }


}

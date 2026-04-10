using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Showroom.Backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/users")
                             .WithTags("Users")
                             .RequireRateLimiting("RateLimit")
                             .RequireAuthorization();

        group.MapGet("{id:int}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Restituisce il profilo di un utente per ID");

        // (Opzionale) Aggiungi GetAll per gli Admin
        group.MapGet("", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Restituisce tutti gli utenti (Solo Admin)")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

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

    private static async Task<Results<Ok<IEnumerable<UserDto>>, NotFound, ForbidHttpResult>> GetAllUsers(IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userPrincipal)
    {

        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching all users");

        var tokenIdString = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? userPrincipal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        bool isAdmin = userPrincipal.IsInRole("Admin");

        if (!isAdmin)
        {
            return TypedResults.Forbid();
        }

        var result = await service.GetAllAsync();

        if (result is null)
        {
            logger.LogWarning("Users not found");
            return TypedResults.NotFound();
        }

        logger.LogInformation("Some users were found");
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<UserDto>, NotFound, ForbidHttpResult>> GetUserById(int id, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userPrincipal)
    {

        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching user by ID: {UserId}", id);

        var tokenIdString = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? userPrincipal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        bool isAdmin = userPrincipal.IsInRole("Admin");

        if (tokenIdString != id.ToString() && !isAdmin)
        {
            return TypedResults.Forbid();
        }

        var result = await service.GetByIdAsync(id);

        if (result is null)
        {
            logger.LogWarning("User not found for ID: {UserId}", id);
            return TypedResults.NotFound();
        }

        logger.LogInformation("User found for ID: {UserId}", id);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IEnumerable<TicketDto>>, ForbidHttpResult, NotFound>> GetUserTickets(int userId, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching tickets for user ID: {UserId}, Culture: {Culture}", userId, culture);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.GetTicketsAsync(userId, culture);

        if (result is null)
        {
            logger.LogWarning("Tickets not found for user ID: {UserId}", userId);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Tickets found for user ID: {UserId}", userId);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<IEnumerable<OrderDto>>, ForbidHttpResult, NotFound>> GetUserOrders(int userId, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching orders for user ID: {UserId}, Culture: {Culture}", userId, culture);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.GetOrdersAsync(userId, culture);

        if (result is null)
        {
            logger.LogWarning("Orders not found for user ID: {UserId}", userId);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Orders found for user ID: {UserId}", userId);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OrderDto>, ForbidHttpResult, NotFound>> GetUserOrderById(int userId, int orderId, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching order ID: {OrderId} for user ID: {UserId}, Culture: {Culture}", orderId, userId, culture);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.GetOrderByIdAsync(userId, orderId, culture);

        if (result is null)
        {
            logger.LogWarning("Order not found - User ID: {UserId}, Order ID: {OrderId}", userId, orderId);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Order found - User ID: {UserId}, Order ID: {OrderId}", userId, orderId);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CartDto>, ForbidHttpResult, NotFound>> GetUserCart(int userId, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Fetching cart for user ID: {UserId}, Culture: {Culture}", userId, culture);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.GetCartAsync(userId, culture);

        if (result is null)
        {
            logger.LogWarning("Cart not found for user ID: {UserId}", userId);
            return TypedResults.NotFound();
        }

        logger.LogInformation("Cart found for user ID: {UserId}", userId);
        return TypedResults.Ok(result);
    }


}

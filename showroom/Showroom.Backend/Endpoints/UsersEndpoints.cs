using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        group.MapGet("", GetAllUsers)
            .WithName("GetAllUsers")
            .WithSummary("Restituisce tutti gli utenti (Solo Admin)")
            .RequireAuthorization(policy => policy.RequireRole("Admin")); // solo admin

        group.MapGet("{userId:int}/cart", GetUserCart)
            .WithName("GetUserCart")
            .WithSummary("Restituisce il carrello di un utente per ID");

        group.MapPost("{userId:int}/cart/items", AddItemsToCart) // aggiunta di articoli al carrello, se non esiste lo crea, se esiste aggiunge o aggiorna la quantità
            .WithName("AddItemsToCart")
            .WithSummary("Aggiunge articoli al carrello di un utente per ID");

        group.MapDelete("{userId:int}/cart/items/{souvenirId:int}", RemoveItemsFromCart) // rimozione di articoli dal carrello
            .WithName("RemoveItemsFromCart")
            .WithSummary("Rimuove articoli dal carrello di un utente per ID");

        group.MapPost("{userId:int}/cart/checkout", CheckoutCart) // creazione dell'ordine e svuotamento del carrello
            .WithName("CheckoutCart")
            .WithSummary("Effettua il checkout del carrello di un utente per ID");

        group.MapGet("{userId:int}/tickets", GetUserTickets)
            .WithName("GetUserTickets")
            .WithSummary("Restituisce i biglietti di un utente per ID");

        group.MapPost("{userId:int}/tickets", CreateTicket)
            .WithName("CreateTicket")
            .WithSummary("Crea un biglietto per un utente per ID");

        group.MapGet("{userId:int}/orders", GetUserOrders)
            .WithName("GetUserOrders")
            .WithSummary("Restituisce gli ordini di un utente per ID");

        group.MapGet("{userId:int}/orders/{orderId:int}", GetUserOrderById)
            .WithName("GetUserOrderById")
            .WithSummary("Restituisce un ordine di un utente per ID");

        //group.MapDelete("/users/{id:int}", DeleteUser)
        //    .WithName("DeleteUser")
        //    .WithDescription("Elimina un utente per ID");
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

    private static async Task<Results<Ok<TicketDto>, ForbidHttpResult, NotFound>> CreateTicket(int userId, CreateTicketDto request, ITicketService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Creating ticket for user ID: {UserId}, Culture: {Culture}", userId, culture);
        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");
        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        request.UserId = userId;
        
        var result = await service.CreateAsync(request, culture);
        if (result is null)
        {
            logger.LogWarning("Failed to create ticket for user ID: {UserId}", userId);
            return TypedResults.NotFound();
        }
        logger.LogInformation("Ticket created for user ID: {UserId}", userId);
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

    private static async Task<Results<Ok<CartItemDto>, ForbidHttpResult, NotFound>> AddItemsToCart(int userId, AddCartItemDto request, ICartService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims, string culture = "en")
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Adding items to cart for user ID: {UserId}, Culture: {Culture}", userId, culture);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.AddItemAsync(request, userId, culture);
        if (result is null)
        {
            logger.LogWarning("Cart not found for user ID: {UserId}", userId);
            return TypedResults.NotFound();
        }
        logger.LogInformation("Items added to cart for user ID: {UserId}", userId);
        return TypedResults.Ok(result);
    }
    private static async Task<Results<Ok<bool>, ForbidHttpResult, NotFound>> RemoveItemsFromCart(int userId, int souvenirId, ICartService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims)
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Removing item from cart for user ID: {UserId}, Souvenir ID: {SouvenirId}", userId, souvenirId);

        var tokenIdString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");

        if (tokenIdString != userId.ToString() && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", userId, tokenIdString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.RemoveItemAsync(userId, souvenirId);
        if (result is false)
        {
            logger.LogWarning("Item not found in cart for user ID: {UserId}, Souvenir ID: {SouvenirId}", userId, souvenirId);
            return TypedResults.NotFound();
        }
        logger.LogInformation("Items removed from cart for user ID: {UserId}", userId);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok, BadRequest, ForbidHttpResult>> CheckoutCart(
    int userId,
    [FromQuery] bool paymentSuccess,
    ICartService service,
    ClaimsPrincipal userPrincipal,
    ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("UserEndpoints");
        logger.LogInformation("Checkout attempt for user ID: {UserId}, Payment Success: {PaymentSuccess}", userId, paymentSuccess);

        var tokenIdString = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? userPrincipal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (tokenIdString != userId.ToString())
        {
            logger.LogWarning("Access denied to checkout - Requested user ID: {RequestedUserId}, Token user ID: {TokenUserId}", userId, tokenIdString);
            return TypedResults.Forbid();
        }

        var result = await service.CheckoutAsync(userId, paymentSuccess);

        if (!result)
        {
            logger.LogWarning("Checkout failed for user ID: {UserId}", userId);
            return TypedResults.BadRequest();
        }

        logger.LogInformation("Checkout successful for user ID: {UserId}", userId);
        return TypedResults.Ok();
    }

}
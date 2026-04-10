using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/auth")
                         .WithTags("Auth")
                         .RequireRateLimiting("BaseRule");

        group.MapPost("login", LoginUser)
            .WithName("LoginUser")
            .WithSummary("Effettua il login di un utente");

        group.MapPost("register", RegisterUser)
            .WithName("RegisterUser")
            .WithSummary("Registra un nuovo utente");

        group.MapPatch("change-password", ChangePasswordUser)
            .WithName("ChangePasswordUser")
            .WithSummary("Cambia la password di un utente");

        group.MapPost("logout", LogoutUser)
            .WithName("LogoutUser")
            .WithSummary("Effettua il logout eliminando il cookie");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> LoginUser(LoginUserDto request, IUserService service, ILoggerFactory loggerFactory, HttpContext context)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var token = await service.LoginAsync(request);
        if (token is null)
        {
            logger.LogWarning("Login failed for email: {Email} - Invalid credentials", request.Email);
            return TypedResults.BadRequest("Credenziali non valide");
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(2)
        };

        context.Response.Cookies.Append("jwt_token", token, cookieOptions);
        logger.LogInformation("Login successful for email: {Email}", request.Email);

        return TypedResults.Ok("Login effettuato con successo");
    }

    public static Results<Ok<string>, BadRequest> LogoutUser(ILoggerFactory loggerFactory, HttpContext context)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Logout attempt");
        context.Response.Cookies.Delete("jwt_token");
        logger.LogInformation("Logout successful");
        return TypedResults.Ok("Logout effettuato");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> RegisterUser(CreateUserDto request, IUserService service, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var result = await service.RegisterAsync(request);
        if (!result)
        {
            logger.LogWarning("Registration failed for email: {Email} - User may already exist", request.Email);
            return TypedResults.BadRequest("Registrazione fallita o utente già esistente");
        }

        logger.LogInformation("Registration successful for email: {Email}", request.Email);
        return TypedResults.Ok("Registrazione avvenuta con successo");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> ChangePasswordUser(ChangePasswordUserDto request, IUserService service, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Password change attempt for email: {Email}", request.Email);

        var result = await service.ChangePasswordAsync(request);
        if (!result)
        {
            logger.LogWarning("Password change failed for email: {Email} - Invalid credentials or user not found", request.Email);
            return TypedResults.BadRequest("Cambio password fallito: credenziali errate o utente non trovato");
        }

        logger.LogInformation("Password change successful for email: {Email}", request.Email);
        return TypedResults.Ok("Password cambiata con successo");
    }
}
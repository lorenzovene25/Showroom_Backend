using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services.Interfaces;
using Showroom.Backend.Utilities;
using Showroom.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Showroom.Backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/auth")
                         .WithTags("Auth")
                         .RequireRateLimiting("AuthLimit");

        group.MapPost("login", LoginUser)
            .WithName("LoginUser")
            .WithSummary("Effettua il login di un utente");

        group.MapPost("register", RegisterUser)
            .WithName("RegisterUser")
            .WithSummary("Registra un nuovo utente");

        group.MapPatch("change-password", ChangePasswordUser)
            .WithName("ChangePasswordUser")
            .WithSummary("Cambia la password di un utente")
            .RequireAuthorization();

        group.MapPost("logout", LogoutUser)
            .WithName("LogoutUser")
            .WithSummary("Effettua il logout eliminando il cookie")
            .RequireAuthorization();
    }

    public static async Task<Results<Ok<UserDto>, BadRequest<string>>> LoginUser(LoginUserDto request, IUserService service, ILoggerFactory loggerFactory, HttpContext context, IConfiguration configuration)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Login attempted");

        var token = await service.LoginAsync(request);
        if (token is null)
        {
            logger.LogWarning("Login failed: Invalid credentials");
            return TypedResults.BadRequest("Credenziali non valide");
        }

        var user = await service.GetByEmailAsync(request.Email!);
        if (user is null)
        {
            logger.LogWarning("User not found");
            return TypedResults.BadRequest("Utente non trovato");
        }

        // Ottiene la durata di scadenza del token dalla configurazione
        var expiryInMinutes = TokenExpirationHelper.GetTokenExpiryInMinutes(configuration);
        var cookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(expiryInMinutes);

        context.Response.Cookies.Append("jwt_token", token, cookieOptions);
        logger.LogInformation("Login successful for email: {Email} - User ID: {UserId}. Cookie expires in {Minutes} minutes", request.Email, user.Id, expiryInMinutes);

        return TypedResults.Ok(user);
    }

    public static async Task<Results<Ok<string>, BadRequest>> LogoutUser(ILoggerFactory loggerFactory, HttpContext context, ITokenBlacklistService blacklistService, IConfiguration configuration)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Logout attempt");

        // Ottiene la durata di scadenza del token dalla configurazione
        var expiryInMinutes = TokenExpirationHelper.GetTokenExpiryInMinutes(configuration);
        var blacklistExpirationTime = TokenExpirationHelper.CalculateBlacklistExpiration(expiryInMinutes);

        // Estrae il token dal cookie
        if (context.Request.Cookies.TryGetValue("jwt_token", out var token))
        {
            // Aggiunge il token alla blacklist con il tempo di scadenza allineato alla configurazione
            await blacklistService.AddTokenAsync(token, blacklistExpirationTime);
            logger.LogInformation("Token aggiunto alla blacklist. Expires: {ExpirationTime}", blacklistExpirationTime);
        }
        else
        {
            // Prova a leggere il token dall'header Authorization
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var bearerToken = authHeader.Substring("Bearer ".Length).Trim();
                await blacklistService.AddTokenAsync(bearerToken, blacklistExpirationTime);
                logger.LogInformation("Token da Authorization header aggiunto alla blacklist. Expires: {ExpirationTime}", blacklistExpirationTime);
            }
        }

        context.Response.Cookies.Delete("jwt_token");
        logger.LogInformation("Logout successful");
        return TypedResults.Ok("Logout effettuato");
    }

    public static async Task<Results<Ok<UserDto>, BadRequest<string>>> RegisterUser(CreateUserDto request, IUserService service, ILoggerFactory loggerFactory, HttpContext context, IConfiguration configuration)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Registration attempt...");

        var result = await service.RegisterAsync(request);
        if (!result)
        {
            logger.LogWarning("Registration failed: User might already exists");
            return TypedResults.BadRequest("Registrazione fallita o utente già esistente");
        }

        var user = await service.GetByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Newly registered user not found for email: {Email}", request.Email);
            return TypedResults.BadRequest("Errore nel recupero dell'utente registrato");
        }

        var loginDto = new LoginUserDto { Email = request.Email, Password = request.Password };
        var token = await service.LoginAsync(loginDto);

        if (token is not null)
        {
            // Ottiene la durata di scadenza del token dalla configurazione
            var expiryInMinutes = TokenExpirationHelper.GetTokenExpiryInMinutes(configuration);
            var cookieOptions = CookieOptionsFactory.CreateJwtCookieOptions(expiryInMinutes);

            context.Response.Cookies.Append("jwt_token", token, cookieOptions);
            logger.LogInformation("Registration and auto-login successful for email: {Email} - User ID: {UserId}. Cookie expires in {Minutes} minutes", request.Email, user.Id, expiryInMinutes);
        }
        else
        {
            logger.LogWarning("Registration successful but auto-login failed for email: {Email}", request.Email);
        }

        return TypedResults.Ok(user);
    }

    public static async Task<Results<Ok<string>, ForbidHttpResult, BadRequest<string>>> ChangePasswordUser(ChangePasswordUserDto request, IUserService service, ILoggerFactory loggerFactory, ClaimsPrincipal userTokenClaims)
    {
        var logger = loggerFactory.CreateLogger("AuthEndpoints");
        logger.LogInformation("Password change attempt for email: {Email}", request.Email);

        var tokenEmailString = userTokenClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? userTokenClaims.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        bool isAdmin = userTokenClaims.IsInRole("Admin");
        if (tokenEmailString != request.Email && !isAdmin)
        {
            logger.LogWarning("Access denied to cart - Token user ID: {TokenUserId}, IsAdmin: {IsAdmin}", tokenEmailString, isAdmin);
            return TypedResults.Forbid();
        }

        var result = await service.ChangePasswordAsync(request);
        if (!result)
        {
            logger.LogWarning("Password change failed for email: {Email} - Invalid credentials or user not found", request.Email);
            return TypedResults.BadRequest("Cambio password fallito");
        }

        logger.LogInformation("Password change successful for email: {Email}", request.Email);
        return TypedResults.Ok("Password cambiata con successo");
    }
}
using Microsoft.AspNetCore.Http; // Assicurati di avere questo
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

        // Ti consiglio di aggiungere anche un endpoint di logout!
        group.MapPost("logout", LogoutUser)
            .WithName("LogoutUser")
            .WithSummary("Effettua il logout eliminando il cookie");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> LoginUser(LoginUserDto request, IUserService service, HttpContext context)
    {
        var token = await service.LoginAsync(request);
        if (token is null)
            return TypedResults.BadRequest("Credenziali non valide");

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(2)
        };

        context.Response.Cookies.Append("jwt_token", token, cookieOptions);

        return TypedResults.Ok("Login effettuato con successo");
    }

    public static Results<Ok<string>, BadRequest> LogoutUser(HttpContext context)
    {
        context.Response.Cookies.Delete("jwt_token");
        return TypedResults.Ok("Logout effettuato");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> RegisterUser(CreateUserDto request, IUserService service)
    {
        var result = await service.RegisterAsync(request);
        if (!result)
            return TypedResults.BadRequest("Registrazione fallita o utente già esistente");
        return TypedResults.Ok("Registrazione avvenuta con successo");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> ChangePasswordUser(ChangePasswordUserDto request, IUserService service)
    {
        var result = await service.ChangePasswordAsync(request);
        if (!result)
            return TypedResults.BadRequest("Cambio password fallito: credenziali errate o utente non trovato");
        return TypedResults.Ok("Password cambiata con successo");
    }
}
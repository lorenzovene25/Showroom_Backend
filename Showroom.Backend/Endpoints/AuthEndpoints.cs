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
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> LoginUser(LoginUserDto request, IUserService service)
    {
        var token = await service.LoginAsync(request.Email, request.Password);
        if (token is null)
            return TypedResults.BadRequest("Credenziali non valide");
        return TypedResults.Ok(token);
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> RegisterUser(LoginUserDto request, IUserService service)
    {
        var result = await service.RegisterAsync(request.Email, request.Password);
        if (!result)
            return TypedResults.BadRequest("Registrazione fallita");
        return TypedResults.Ok("Registrazione avvenuta con successo");
    }

    public static async Task<Results<Ok<string>, BadRequest<string>>> ChangePasswordUser(ChangePasswordUserDto request, IUserService service)
    {
        var result = await service.ChangePasswordAsync(request.Email, request.OldPassword, request.NewPassword);
        if (!result)
            return TypedResults.BadRequest("Cambio password fallito");
        return TypedResults.Ok("Password cambiata con successo");
    }

}
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;

namespace Showroom.Backend.Endpoints;

// ══════════════════════════════════════════════════════════════════
//  UTENTI  (Users)
// ══════════════════════════════════════════════════════════════════
public static class UserEndpoints
{
public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /users/email/{email}
        app.MapGet("/users/email/{email}", async (string email, IUserService svc) =>
        {
            var result = await svc.GetByEmailAsync(email);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetUserByEmail")
        .WithTags("Users")
        .WithSummary("Restituisce un utente per email")
        .Produces<UserDto>()
        .ProducesProblem(404);

        // GET /users/{id}
        app.MapGet("/users/{id:int}", async (int id, IUserService svc) =>
        {
            var result = await svc.GetByIdAsync(id);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .WithSummary("Restituisce un utente per ID")
        .Produces<UserDto>()
        .ProducesProblem(404);

        // POST /users
        // NOTA: l'hashing della password deve avvenire PRIMA di chiamare questo endpoint
        // (es. BCrypt nel layer auth); il service salva direttamente il valore ricevuto.
        app.MapPost("/users", async (CreateUserDto dto, IUserService svc) =>
        {
            var created = await svc.CreateAsync(dto);
            return Results.Created($"/users/{created.Id}", created);
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithSummary("Crea un nuovo utente (con cart atomica)")
        .Produces<UserDto>(201);

        // PATCH /users/{id}
        app.MapPatch("/users/{id:int}", async (int id, PatchUserDto dto, IUserService svc) =>
        {
            var result = await svc.PatchAsync(id, dto);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("PatchUser")
        .WithTags("Users")
        .WithSummary("Aggiorna parzialmente un utente")
        .Produces<UserDto>()
        .ProducesProblem(404);

        // DELETE /users/{id}
        app.MapDelete("/users/{id:int}", async (int id, IUserService svc) =>
        {
            var deleted = await svc.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithSummary("Elimina un utente")
        .Produces(204)
        .ProducesProblem(404);
    }
}

namespace Showroom.Backend.Endpoints;

public class AdminEndpoints
{
    public static void MapAdminEndpoints(IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/admin")
            .WithGroupName("Admin")
            .WithDescription("Endpoints for admin users")
            .RequireAuthorization("AdminPolicy");

        group.MapPost("/exhibitions", CreateExhibition)
            .WithName("CreateExhibition")
            .WithDescription("Crea una nuova mostra");

        group.MapPost("/artworks", CreateArtwork)
            .WithName("CreateArtwork")
            .WithDescription("Crea una nuova opera d'arte");

        group.MapPost("/souvenirs", CreateSouvenir)
            .WithName("CreateSouvenir")
            .WithDescription("Crea un nuovo souvenir");

        group.MapPut("/exhibitions", UpdateExhibition)
            .WithName("UpdateExhibition")
            .WithDescription("Aggiorna una mostra esistente");

        group.MapPut("/artworks", UpdateArtwork)
            .WithName("UpdateArtwork")
            .WithDescription("Aggiorna un'opera d'arte esistente");

        group.MapPut("/souvenirs", UpdateSouvenir)
            .WithName("UpdateSouvenir")
            .WithDescription("Aggiorna un souvenir esistente");

        group.MapDelete("/exhibitions", DeleteExhibition)
            .WithName("DeleteExhibition")
            .WithDescription("Elimina una mostra esistente");

        group.MapDelete("/artworks", DeleteArtwork)
            .WithName("DeleteArtwork")
            .WithDescription("Elimina un'opera d'arte esistente");

        group.MapDelete("/souvenirs", DeleteSouvenir)
            .WithName("DeleteSouvenir")
            .WithDescription("Elimina un souvenir esistente");

    }

    private static Task<IResult> CreateExhibition() => throw new NotImplementedException();
    private static Task<IResult> CreateArtwork() => throw new NotImplementedException();
    private static Task<IResult> CreateSouvenir() => throw new NotImplementedException();
    private static Task<IResult> UpdateExhibition() => throw new NotImplementedException();
    private static Task<IResult> UpdateArtwork() => throw new NotImplementedException();
    private static Task<IResult> UpdateSouvenir() => throw new NotImplementedException();
    private static Task<IResult> DeleteExhibition() => throw new NotImplementedException();
    private static Task<IResult> DeleteArtwork() => throw new NotImplementedException();
    private static Task<IResult> DeleteSouvenir() => throw new NotImplementedException();
}
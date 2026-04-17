using Microsoft.AspNetCore.Http.HttpResults;
using Showroom.Backend.Dtos;
using Showroom.Backend.Services;
using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Endpoints;

public static class MediaEndpoints
{
    public static void MapMediaEndpoints(this IEndpointRouteBuilder route)
    {
        var group = route.MapGroup("/api/previews")
            .WithTags("Previews")
            .RequireRateLimiting("RateLimit");

        // GET /api/previews/artists
        group.MapGet("artists", GetArtistImagesAsync)
            .WithName("GetArtistImages")
            .WithSummary("Restituisce tutte le immagini degli artisti mappate con i loro nomi");

        // GET /api/previews/artists/{artistName}/preview
        group.MapGet("artists/{artistName}/preview", GetArtistPreviewAsync)
            .WithName("GetArtistPreview")
            .WithSummary("Restituisce il path del preview di un artista");

    }

    /// <summary>
    /// GET /api/media/artists
    /// Restituisce un dizionario con nomi artisti e path preview
    /// </summary>
    public static async Task<Results<Ok<Dictionary<string, string>>, NotFound<string>>> GetArtistImagesAsync(
        IMediaService mediaService,
        ILoggerFactory loggerFactory)
    {
        var result = await mediaService.GetArtistsPreview();
        return result is Dictionary<string, string>
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Errore durante il recupero delle immagini degli artisti");
    }

    /// <summary>
    /// GET /api/media/artists/{artistName}/preview
    /// Restituisce solo il path del preview
    /// </summary>
    public static async Task<Results<Ok<string>, NotFound>> GetArtistPreviewAsync(
        string artistName,
        IMediaService mediaService,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("MediaEndpoints");
        logger.LogInformation("Richiesta preview artista: {ArtistName}", artistName);

        var previewPath = await mediaService.GetArtistPreview(artistName.Replace(" ", "_"));

        if (previewPath is null)
        {
            logger.LogWarning("Preview non trovato per artista: {ArtistName}", artistName);
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(previewPath);
    }

}

using Showroom.Backend.Services.Interfaces;

namespace Showroom.Backend.Services;

/// <summary>
/// Servizio per gestire il recupero di immagini dalla cartella media
/// </summary>
public class MediaService : IMediaService
{
    private readonly string _mediaPath;
    private readonly ILogger<MediaService> _logger;

    public MediaService(IWebHostEnvironment env, ILogger<MediaService> logger)
    {
        _mediaPath = Path.Combine(env.WebRootPath, "media");
        _logger = logger;
    }

    /// <summary>
    /// Ottiene il path dell'immagine di preview per un artista
    /// </summary>
    public async Task<string?> GetArtistPreview(string artistName)
    {
        try
        {
            var previewPath = Path.Combine(_mediaPath, artistName, "preview.jpg");

            if (File.Exists(previewPath))
            {
                var relativePath = Path.Combine("media", artistName, "preview.jpg")
                    .Replace("\\", "/");
                _logger.LogDebug("Preview trovato per artista {Artist}: {Path}", artistName, relativePath);
                return await Task.FromResult(relativePath);
            }

            _logger.LogWarning("Preview non trovato per artista {Artist}", artistName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero preview per artista {Artist}", artistName);
            return null;
        }
    }

    public async Task<Dictionary<string, string>> GetArtistsPreview()
    {
        var previews = new Dictionary<string, string>();
        try
        {
            if (Directory.Exists(_mediaPath))
            {
                var artistDirs = Directory.GetDirectories(_mediaPath);
                foreach (var dir in artistDirs)
                {
                    var artistName = Path.GetFileName(dir);
                    var previewPath = Path.Combine(dir, "preview.jpg");
                    if (File.Exists(previewPath))
                    {
                        var relativePath = Path.Combine("media", artistName, "preview.jpg")
                            .Replace("\\", "/");
                        previews[artistName] = relativePath;
                        _logger.LogDebug("Preview trovato per artista {Artist}: {Path}", artistName, relativePath);
                    }
                    else
                    {
                        _logger.LogWarning("Preview non trovato per artista {Artist}", artistName);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Cartella media non trovata: {MediaPath}", _mediaPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero delle preview degli artisti");
        }
        return await Task.FromResult(previews);
    }
}

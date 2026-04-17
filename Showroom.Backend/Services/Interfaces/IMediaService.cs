namespace Showroom.Backend.Services.Interfaces;

/// <summary>
/// Servizio per gestire il recupero di immagini dalla cartella media
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Ottiene l'immagine di preview per un artista
    /// </summary>
    /// <param name="artistName">Nome della cartella dell'artista</param>
    /// <returns>Path relativo dell'immagine di preview</returns>
    Task<string?> GetArtistPreview(string artistName);
    Task<Dictionary<string, string>> GetArtistsPreview();
}

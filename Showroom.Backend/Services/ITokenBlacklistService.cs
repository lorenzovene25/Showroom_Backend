namespace Showroom.Backend.Services;

public interface ITokenBlacklistService
{
    /// <summary>
    /// Aggiunge un token alla blacklist
    /// </summary>
    Task AddTokenAsync(string token, DateTime expirationTime);

    /// <summary>
    /// Verifica se un token è nella blacklist
    /// </summary>
    Task<bool> IsTokenBlacklistedAsync(string token);

    /// <summary>
    /// Rimuove i token scaduti dalla blacklist (cleanup)
    /// </summary>
    Task RemoveExpiredTokensAsync();
}

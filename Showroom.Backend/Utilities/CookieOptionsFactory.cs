namespace Showroom.Backend.Utilities;

/// <summary>
/// Factory per creare CookieOptions con configurazioni di sicurezza consistenti
/// </summary>
public static class CookieOptionsFactory
{
    /// <summary>
    /// Crea le opzioni per il cookie JWT con scadenza allineata al token
    /// </summary>
    public static CookieOptions CreateJwtCookieOptions(double expiryInMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = TokenExpirationHelper.CalculateCookieExpiration(expiryInMinutes),
            // Garantisce che il cookie venga cancellato quando il browser si chiude
            // Tuttavia, abbiamo comunque un Expires come fallback
            IsEssential = true
        };
    }
}

namespace Showroom.Backend.Utilities;

/// <summary>
/// Helper per calcolare le scadenze dei cookie e token JWT
/// </summary>
public static class TokenExpirationHelper
{
    /// <summary>
    /// Ottiene la durata di scadenza del token JWT dalla configurazione
    /// </summary>
    public static double GetTokenExpiryInMinutes(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        return Convert.ToDouble(jwtSettings["ExpiryInMinutes"] ?? "120");
    }

    /// <summary>
    /// Calcola il tempo di scadenza del cookie in base alla durata del token
    /// </summary>
    public static DateTimeOffset CalculateCookieExpiration(double expiryInMinutes)
    {
        return DateTimeOffset.UtcNow.AddMinutes(expiryInMinutes);
    }

    /// <summary>
    /// Calcola il tempo di scadenza della blacklist in base alla durata del token
    /// </summary>
    public static DateTime CalculateBlacklistExpiration(double expiryInMinutes)
    {
        return DateTime.UtcNow.AddMinutes(expiryInMinutes);
    }
}

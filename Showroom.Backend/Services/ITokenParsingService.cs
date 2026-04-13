using System.IdentityModel.Tokens.Jwt;

namespace Showroom.Backend.Services;

/// <summary>
/// Servizio per estrarre informazioni dai token JWT
/// </summary>
public interface ITokenParsingService
{
    /// <summary>
    /// Estrae la scadenza effettiva dal token JWT
    /// </summary>
    DateTime? GetTokenExpiration(string token);

    /// <summary>
    /// Verifica se il token è ancora valido
    /// </summary>
    bool IsTokenValid(string token);
}

/// <summary>
/// Implementazione del servizio di parsing dei token JWT
/// </summary>
public class TokenParsingService : ITokenParsingService
{
    private readonly ILogger<TokenParsingService> _logger;

    public TokenParsingService(ILogger<TokenParsingService> logger)
    {
        _logger = logger;
    }

    public DateTime? GetTokenExpiration(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var jwtHandler = new JwtSecurityTokenHandler();
            
            // Validiamo che sia un token JWT valido
            if (!jwtHandler.CanReadToken(token))
            {
                _logger.LogWarning("Token non è un JWT valido");
                return null;
            }

            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Errore nell'estrazione della scadenza dal token");
            return null;
        }
    }

    public bool IsTokenValid(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var jwtHandler = new JwtSecurityTokenHandler();
            
            if (!jwtHandler.CanReadToken(token))
                return false;

            var jwtToken = jwtHandler.ReadJwtToken(token);
            return jwtToken.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using Showroom.Backend.Services;

namespace Showroom.Backend.Extensions;

/// <summary>
/// Middleware per verificare se il token JWT è nella blacklist
/// </summary>
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenBlacklistMiddleware> _logger;

    public TokenBlacklistMiddleware(RequestDelegate next, ILogger<TokenBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklistService, ITokenParsingService tokenParsingService)
    {
        // Estrae il token dal cookie o dall'header Authorization
        var token = ExtractToken(context);

        if (!string.IsNullOrWhiteSpace(token))
        {
            var isBlacklisted = await blacklistService.IsTokenBlacklistedAsync(token);
            if (isBlacklisted)
            {
                _logger.LogWarning("Token nella blacklist rilevato");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token revocato. Effettua il login di nuovo." });
                return;
            }

            // Log della scadenza del token per debug
            var tokenExpiration = tokenParsingService.GetTokenExpiration(token);
            if (tokenExpiration.HasValue)
            {
                var timeRemaining = tokenExpiration.Value - DateTime.UtcNow;
                _logger.LogDebug("Token valido. Scade in {TimeRemaining} minuti", timeRemaining.TotalMinutes);
            }
        }

        await _next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        // Prova a leggere il token dal cookie
        if (context.Request.Cookies.TryGetValue("jwt_token", out var cookieToken))
        {
            return cookieToken;
        }

        // Prova a leggere il token dall'header Authorization
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}

/// <summary>
/// Estensioni per aggiungere il middleware di blacklist
/// </summary>
public static class TokenBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenBlacklistMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TokenBlacklistMiddleware>();
    }
}

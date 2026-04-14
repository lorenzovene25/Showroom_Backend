using Showroom.Backend.Services.Interfaces;
using System.Collections.Concurrent;

namespace Showroom.Backend.Services;

/// <summary>
/// Servizio per gestire la blacklist dei token JWT in-memory
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();
    private readonly ILogger<TokenBlacklistService> _logger;

    public TokenBlacklistService(ILogger<TokenBlacklistService> logger)
    {
        _logger = logger;
    }

    public Task AddTokenAsync(string token, DateTime expirationTime)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Tentativo di aggiungere un token vuoto alla blacklist");
            return Task.CompletedTask;
        }

        // Generiamo un hash del token per ridurre l'uso di memoria
        var tokenHash = GenerateTokenHash(token);
        _blacklistedTokens.TryAdd(tokenHash, expirationTime);
        _logger.LogInformation("Token aggiunto alla blacklist con scadenza: {ExpirationTime}", expirationTime);

        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        var tokenHash = GenerateTokenHash(token);
        var isBlacklisted = _blacklistedTokens.TryGetValue(tokenHash, out var expirationTime);

        if (isBlacklisted && expirationTime > DateTime.UtcNow)
        {
            _logger.LogWarning("Token trovato nella blacklist");
            return Task.FromResult(true);
        }

        // Se il token è scaduto nella blacklist, lo rimuoviamo
        if (isBlacklisted && expirationTime <= DateTime.UtcNow)
        {
            _blacklistedTokens.TryRemove(tokenHash, out _);
        }

        return Task.FromResult(false);
    }

    public Task RemoveExpiredTokensAsync()
    {
        var expiredTokens = _blacklistedTokens
            .Where(kvp => kvp.Value <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var tokenHash in expiredTokens)
        {
            _blacklistedTokens.TryRemove(tokenHash, out _);
        }

        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation("Rimossi {Count} token scaduti dalla blacklist", expiredTokens.Count);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Genera un hash del token per ridurre l'uso di memoria
    /// </summary>
    private static string GenerateTokenHash(string token)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

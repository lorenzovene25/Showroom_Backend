using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Showroom.Backend.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };


            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.TryGetValue("jwt_token", out var token))
                    {
                        context.Token = token;
                        Console.WriteLine($"[AUTH] Cookie 'jwt_token' trovato!");
                    }
                    else
                    {
                        Console.WriteLine("[AUTH] Nessun cookie 'jwt_token' trovato nella richiesta.");
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null) Console.WriteLine($"[AUTH] Trovato Header Authorization: {authHeader}");
                    }
                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"[AUTH] Autenticazione FALLITA!");

                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        Console.WriteLine("[AUTH] Il token è SCADUTO.");
                    }
                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine($"[AUTH] Token VALIDATO con successo per l'utente: {context.Principal?.Identity?.Name}");
                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    Console.WriteLine($"[AUTH] Challenge inviata al client...");
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();

        return services;
    }
}
using Microsoft.AspNetCore.RateLimiting;
using Showroom.Backend.Services;
using Showroom.Backend.Security;

namespace Showroom.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    // Registra tutti i servizi
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IArtworkService, ArtworkService>();
        services.AddScoped<IExhibitionService, ExhibitionService>();
        services.AddScoped<ISouvenirService, SouvenirService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketTierService, TicketTierService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();

        //provider del token JWT
        services.AddScoped<IJwtProvider, JwtProvider>();

        // Servizio per la blacklist dei token JWT
        services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

        return services;
    }

    // CORS Configuration
    public static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    // Rate Limiter
    public static IServiceCollection AddRateLimitingConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("RateLimit", limiterOptions =>
            {
                limiterOptions.PermitLimit = 100;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });
            options.RejectionStatusCode = 429;
        });

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("AuthLimit", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });
            options.RejectionStatusCode = 429;
        });

        return services;
    }
}
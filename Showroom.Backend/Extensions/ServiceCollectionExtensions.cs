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

        return services;
    }
}
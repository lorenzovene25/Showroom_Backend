using Microsoft.AspNetCore.Diagnostics;
using PW.WebApi.Endpoints;
using Showroom.Backend.Endpoints;

namespace Showroom.Backend.Extensions;

public static class WebApplicationExtensions
{
    // Mappa tutti gli endpoint 
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapArtworkEndpoints();

        app.MapExhibitionEndpoints();

        app.MapSouvenirEndpoints();

        app.MapUserEndpoints();

        app.MapCategoryEndpoints();

        app.MapAuthEndpoints();

        app.MapMediaEndpoints();

        return app;
    }

    // gestore globale degli errori
    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                                                   .CreateLogger("GlobalExceptionHandler");

                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error is Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception caught by global handler");
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = "Si è verificato un errore interno al server."
                };

                await context.Response.WriteAsJsonAsync(response);
            });
        });

        return app;
    }

}
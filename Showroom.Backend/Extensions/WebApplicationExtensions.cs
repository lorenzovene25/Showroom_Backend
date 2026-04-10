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

        return app;
    }

    // gestore globale degli errori
    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Si è verificato un errore interno al server." });
            });
        });

        return app;
    }
}
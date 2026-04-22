using Microsoft.AspNetCore.HttpOverrides;
using Showroom.Backend.Extensions;

// Register Dapper type handlers for DateOnly and TimeOnly
DapperSqlHandlersExtensions.AddDapperTypeHandlers();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddRateLimitingConfig(); // rate limiter
builder.Services.AddCorsConfig(builder.Configuration); // CORS configuration
builder.Services.AddJwtAuthentication(builder.Configuration); // JWT
builder.Services.AddApplicationServices(); // injection degli IService

var app = builder.Build();

// per mandare richieste anche se in http con nginx
// normalmente le fa in https ma se viene dal reverse proxy le manda lo stesso
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Svuotiamo le reti "conosciute" per fidarci del proxy Docker
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseGlobalExceptionHandler(); // gestione globale degli errori

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Products API v1");
    });
}

string[] honeypots = ["/api/wp-admin", "/api/.env", "/api/phpmyadmin", "/api/config.php", "/api/.git"];

foreach (var path in honeypots)
{
    app.Map(path, async (HttpContext context, ILogger<Program> logger) =>
    {
        //dati dell'attaccante
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var method = context.Request.Method;

        logger.LogWarning($"HONEYPOT! IP: {ip} ha provato un {method} su {path}. Tool: {userAgent}");

        // delay di 5 secondi
        await Task.Delay(5000);

        // risposta finta per wp-admin 1 byte ognu secondo
        if (path == "/api/wp-admin")
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";

            string message = "{ \"status\": \"success\", \"loot\": \"1000 bottiglie di Baby Oil \\U0001F9F4\\U0001F476\\U0001F4A7\" }";
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

            context.Response.Headers.ContentLength = messageBytes.Length;

            foreach (byte b in messageBytes)
            {
                try
                {
                    await context.Response.Body.WriteAsync(new[] { b }, 0, 1, context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);

                    await Task.Delay(50, context.RequestAborted);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        //fake .env 
        if (path == "/api/.env")
        {
            context.Response.ContentType = "text/plain";
            string fakeEnv = """
            DB_CONNECTION=mysql
            DB_HOST=10.0.0.45
            DB_PORT=3306
            DB_DATABASE=production_db
            DB_USERNAME=admin_root
            DB_PASSWORD=Scherzetto_Questa_Password_Non_Funziona_123!
            AWS_ACCESS_KEY_ID=AKIAIOSFODNN7EXAMPLE
            AWS_SECRET_ACCESS_KEY=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
            """;

            return Results.Text(fakeEnv);
        }

        // Per tutti gli altri percorsi ritorna 418
        return Results.Json(new
        {
            status = 418,
            message = "Nice try Diddy \U0001F9F4\U0001F476\U0001F4A7"
        }, statusCode: 418);
    });
}

app.UseCors("AllowFrontend"); // CORS middleware

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSecurityHeaders();

app.UseTokenBlacklistMiddleware(); // Middleware per verificare la blacklist dei token

app.UseAuthentication();
app.UseAuthorization();

// Middleware per rilevare possibili attacchi SQL injection nei body delle richieste POST, PUT e PATCH
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    if (method == HttpMethods.Post || method == HttpMethods.Put || method == HttpMethods.Patch)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var bodyContent = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        var upperBody = bodyContent.ToUpperInvariant();
        if (upperBody.Contains("' OR 1=1") || upperBody.Contains("DROP TABLE"))
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            string fakeError = """
            {
                "error": "System.Data.SqlClient.SqlException",
                "message": "Syntax error near 'Guanciale'.",
                "stackTrace": "at NonUsareLaPancetta() in C:\\Ricette\\Carbonara.cs:line 42\n  at NientePanna() in C:\\Ricette\\Carbonara.cs:line 84\n  at DaiCheCsharpETypeSafe() in C:\\Backend\\Program.cs"
            }
            """;

            await context.Response.WriteAsync(fakeError);

            return;
        }
    }

    await next(context);
});

app.MapApplicationEndpoints(); // Mappa gli endpoint

app.Run();
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

app.UseCors("AllowFrontend"); // CORS middleware

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSecurityHeaders();

app.UseTokenBlacklistMiddleware(); // Middleware per verificare la blacklist dei token

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints(); // Mappa gli endpoint

app.Run();
using Microsoft.AspNetCore.HttpOverrides;
using Showroom.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddRateLimitingConfig(); // rate limiter
builder.Services.AddJwtAuthentication(builder.Configuration); // JWT
builder.Services.AddApplicationServices(); // injection degli IService

var app = builder.Build();

// per mandare richieste anche se in http con nginx
// normalmente le fa in https ma se viene dal reverse proxy le manda lo stesso
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseGlobalExceptionHandler(); // gestione globale degli errori

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Products API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints(); // Mappa gli endpoint

app.Run();
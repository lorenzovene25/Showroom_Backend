using Microsoft.AspNetCore.RateLimiting;
using PW.WebApi.Endpoints;
using Showroom.Backend;
using Showroom.Backend.Endpoints;
using Showroom.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("RateLimit", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

builder.Services.AddOpenApi();

builder.Services.AddScoped<IArtworkService, ArtworkService>();
builder.Services.AddScoped<IExhibitionService,ExhibitionService>();
builder.Services.AddScoped<ISouvenirService,SouvenirService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<ITicketService,TicketService>();
builder.Services.AddScoped<ITicketTierService, TicketTierService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();



var app = builder.Build();

//global error handler per catturare eccezioni non gestite e restituire una risposta JSON standardizzata
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "Si è verificato un errore interno al server." });
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Products API v1");
    });
}

app.UseHttpsRedirection();

app.MapArtworkEndpoints();
app.MapExhibitionEndpoints();
app.MapSouvenirEndpoints();
app.MapUserEndpoints();
app.MapCategoryEndpoints();

app.Run();
using Showroom.Backend;
using Showroom.Backend.Services;
using Showroom.Backend.Endpoints;

var builder = WebApplication.CreateBuilder(args);

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
app.MapTicketEndpoints();

app.Run();
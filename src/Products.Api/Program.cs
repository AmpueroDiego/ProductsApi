using System.Reflection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi;
using Products.Api.Middleware;
using Products.Application.Exceptions;
using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Infrastructure.Cache;
using Products.Infrastructure.Data;
using Products.Infrastructure.External;
using Products.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products API",
        Version = "v1",
        Description = "API REST para la gestión de productos (crear, actualizar y consultar)."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Database: a simple factory that returns a brand new connection per call.
var connectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Missing connection string 'SqlServer'.");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Status cache: TTL read from configuration, never hardcoded.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton(new StatusCacheOptions
{
    TtlMinutes = builder.Configuration.GetValue("StatusCache:TtlMinutes", 5)
});
builder.Services.AddSingleton<IStatusCache, StatusCache>();

// External discount service via HttpClient.
var discountBaseUrl = builder.Configuration["DiscountService:BaseUrl"]
    ?? throw new InvalidOperationException("Missing 'DiscountService:BaseUrl'.");
builder.Services.AddHttpClient<IDiscountService, DiscountService>(client =>
{
    client.BaseAddress = new Uri(discountBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestTimingMiddleware>();

// Global error handling: map domain exceptions to HTTP status codes.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

    var (status, error, message) = exception switch
    {
        ProductNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
        ProductAlreadyExistsException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
    };

    context.Response.StatusCode = status;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { status, error, message });
}));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

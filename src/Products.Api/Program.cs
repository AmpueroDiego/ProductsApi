using FluentValidation;
using Products.Api.Middleware;
using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Application.Validators;
using Products.Infrastructure.Cache;
using Products.Infrastructure.Data;
using Products.Infrastructure.External;
using Products.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

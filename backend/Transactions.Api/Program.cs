using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Transactions.Api.Clients;
using Transactions.Api.Data;
using Transactions.Api.Middleware;
using Transactions.Api.Repositories;
using Transactions.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. Registro de servicios
// =====================================================================

// Hago que los enums viajen como texto ("Purchase", "Sale") en vez de
// números (1, 2): el JSON se entiende solo y el frontend no depende de números mágicos.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TransactionsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TransactionsDb")));

// Cliente tipado hacia Productos con IHttpClientFactory. La URL base viene
// de appsettings.json y pongo un timeout corto para no dejar al usuario
// esperando si Productos no responde.
string productsApiUrl = builder.Configuration["Services:ProductsApi"]
    ?? throw new InvalidOperationException("Falta la configuración Services:ProductsApi.");

builder.Services.AddHttpClient<IProductsApiClient, ProductsApiClient>(client =>
{
    client.BaseAddress = new Uri(productsApiUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS: solo permito el origen del frontend de Angular.
const string FrontendCorsPolicy = "Frontend";
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// =====================================================================
// 2. Pipeline HTTP
// =====================================================================

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI en http://localhost:5002/swagger
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Transactions API v1"));
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();

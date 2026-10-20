using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Transactions.Api.Clients;
using Transactions.Api.Data;
using Transactions.Api.Middleware;
using Transactions.Api.Repositories;
using Transactions.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<TransactionsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TransactionsDb")));

string productsApiUrl = builder.Configuration["Services:ProductsApi"]
    ?? throw new InvalidOperationException("Falta la configuración Services:ProductsApi.");

// Timeout corto para no dejar al usuario esperando si Products.Api no responde.
builder.Services.AddHttpClient<IProductsApiClient, ProductsApiClient>(client =>
{
    client.BaseAddress = new Uri(productsApiUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Transactions API v1"));
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();

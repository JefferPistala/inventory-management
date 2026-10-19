using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Middleware;
using Products.Api.Repositories;
using Products.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. REGISTRO DE SERVICIOS (inyección de dependencias)
// =====================================================================

builder.Services.AddControllers();

// Documento OpenAPI en /openapi/v1.json (lo incluye .NET 10).
builder.Services.AddOpenApi();

// DbContext con SQL Server. La cadena de conexión vive en appsettings.json,
// nunca escrita en el código.
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsDb")));

// Scoped = una instancia por petición HTTP, igual que el DbContext.
// Se registra "interfaz → implementación": las clases piden IProductService
// y el contenedor les entrega ProductService.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Manejo global de errores con formato ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS: el navegador bloquea llamadas de localhost:4200 (Angular) a
// localhost:5001 (esta API) salvo que la API lo permita explícitamente.
// Solo se permite el origen del frontend, leído de la configuración.
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
// 2. PIPELINE HTTP (el orden importa: cada petición pasa por aquí en orden)
// =====================================================================

// Primero: así captura las excepciones de todo lo que viene después.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Interfaz visual de Swagger sobre el documento OpenAPI de .NET 10.
    // Abrir http://localhost:5001/swagger para probar los endpoints.
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Products API v1"));
}

// Sin UseHttpsRedirection: en local trabajamos con HTTP (decisión del proyecto).

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();

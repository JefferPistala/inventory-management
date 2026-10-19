using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Repositories;

// Implementación con EF Core. Aquí viven TODAS las consultas de productos.
public class ProductRepository(ProductsDbContext context) : IProductRepository
{
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query)
    {
        // IQueryable: todavía NO se ejecuta nada; solo se arma la consulta.
        // AsNoTracking: es solo lectura, EF no necesita vigilar cambios (más rápido).
        IQueryable<Product> products = context.Products
            .AsNoTracking()
            .Include(p => p.Category);

        // Filtros dinámicos: cada uno se agrega SOLO si el usuario lo envió.
        if (!string.IsNullOrWhiteSpace(query.Name))
            products = products.Where(p => p.Name.Contains(query.Name.Trim()));

        if (query.CategoryId.HasValue)
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.MinPrice.HasValue)
            products = products.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            products = products.Where(p => p.Price <= query.MaxPrice.Value);

        // El total se cuenta ANTES de paginar: es el total de resultados del filtro.
        int totalCount = await products.CountAsync();

        // OrderBy es obligatorio antes de Skip/Take: sin orden, SQL Server no
        // garantiza el orden y las páginas podrían repetir o saltarse filas.
        // ThenBy(Id) desempata productos con el mismo nombre.
        // Skip/Take se traducen a OFFSET/FETCH: solo viaja la página pedida.
        List<Product> items = await products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(int id) =>
        // Sin AsNoTracking: el Service puede modificar la entidad devuelta
        // y luego llamar a SaveChangesAsync para guardar (editar / eliminar).
        context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> ExistsAsync(int id) =>
        context.Products.AnyAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();

    public async Task<bool> TryAdjustStockAsync(int productId, int delta)
    {
        // Un solo UPDATE con la condición en el WHERE:
        //   UPDATE Products SET Stock = Stock + @delta, UpdatedAt = @now
        //   WHERE Id = @id AND IsActive = 1 AND Stock + @delta >= 0
        // La verificación y la resta ocurren en la MISMA sentencia, así que
        // dos ventas simultáneas no pueden "colarse" entre ambas (sin carrera).
        DateTime now = DateTime.UtcNow;

        int affectedRows = await context.Products
            .Where(p => p.Id == productId && p.Stock + delta >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Stock, p => p.Stock + delta)
                .SetProperty(p => p.UpdatedAt, now));

        return affectedRows == 1;
    }
}

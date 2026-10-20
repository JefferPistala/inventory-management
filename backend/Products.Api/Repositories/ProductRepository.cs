using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Repositories;

public class ProductRepository(ProductsDbContext context) : IProductRepository
{
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query)
    {
        IQueryable<Product> products = context.Products
            .AsNoTracking()
            .Include(p => p.Category);

        if (!string.IsNullOrWhiteSpace(query.Name))
            products = products.Where(p => p.Name.Contains(query.Name.Trim()));

        if (query.CategoryId.HasValue)
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.MinPrice.HasValue)
            products = products.Where(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            products = products.Where(p => p.Price <= query.MaxPrice.Value);

        int totalCount = await products.CountAsync();
        // Sin un orden estable, la paginación con Skip/Take puede repetir u omitir filas.
        List<Product> items = await products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(int id, bool includeInactive = false)
    {
        IQueryable<Product> products = context.Products.Include(p => p.Category);

        if (includeInactive)
            products = products.IgnoreQueryFilters();

        return products.FirstOrDefaultAsync(p => p.Id == id);
    }

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
        // Verifico y ajusto en un solo UPDATE para que dos ventas simultáneas no dejen el stock negativo.
        DateTime now = DateTime.UtcNow;

        int affectedRows = await context.Products
            .Where(p => p.Id == productId && p.Stock + delta >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Stock, p => p.Stock + delta)
                .SetProperty(p => p.UpdatedAt, now));

        return affectedRows == 1;
    }
}

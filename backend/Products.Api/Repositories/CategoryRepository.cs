using Microsoft.EntityFrameworkCore;
using Products.Api.Data;
using Products.Api.Entities;

namespace Products.Api.Repositories;

public class CategoryRepository(ProductsDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync() =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)   // orden explícito: nunca depender del orden "natural"
            .ToListAsync();

    public Task<bool> ExistsAsync(int id) =>
        context.Categories.AnyAsync(c => c.Id == id);
}

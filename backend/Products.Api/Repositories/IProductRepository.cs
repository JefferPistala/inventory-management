using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Repositories;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query);

    Task<Product?> GetByIdAsync(int id, bool includeInactive = false);
    Task<bool> ExistsAsync(int id);

    Task AddAsync(Product product);

    Task SaveChangesAsync();

    // false si el producto no existe o el stock no alcanza; el Service decide qué error lanzar.
    Task<bool> TryAdjustStockAsync(int productId, int delta);
}

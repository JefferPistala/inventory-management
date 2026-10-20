using Products.Api.Dtos;
using Products.Api.Entities;
using Products.Api.Exceptions;
using Products.Api.Mappings;
using Products.Api.Repositories;

namespace Products.Api.Services;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository) : IProductService
{
    public async Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryParameters query)
    {
        if (query.MinPrice > query.MaxPrice)
            throw new BusinessRuleException("El precio mínimo no puede ser mayor que el precio máximo.");

        var (items, totalCount) = await productRepository.GetPagedAsync(query);

        return new PagedResult<ProductResponse>(
            items.Select(p => p.ToResponse()).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    // includeInactive solo aplica a consultas; editar, eliminar y ajustar stock ignoran los eliminados.
    public async Task<ProductResponse> GetByIdAsync(int id, bool includeInactive = false)
    {
        Product product = await productRepository.GetByIdAsync(id, includeInactive)
            ?? throw new NotFoundException($"El producto {id} no existe.");
        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        await EnsureCategoryExistsAsync(request.CategoryId);

        // El inventario inicial se registra con una compra.
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl?.Trim(),
            Price = request.Price,
            Stock = 0
        };

        await productRepository.AddAsync(product);

        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        Product product = await GetExistingProductAsync(id);

        if (product.CategoryId != request.CategoryId)
            await EnsureCategoryExistsAsync(request.CategoryId);

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.Price = request.Price;
        product.UpdatedAt = DateTime.UtcNow;

        // Si otro usuario lo modificó antes, RowVersion no coincide y EF lanza DbUpdateConcurrencyException (409).
        await productRepository.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        Product product = await GetExistingProductAsync(id);

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.SaveChangesAsync();
    }

    public async Task<ProductResponse> AdjustStockAsync(int id, int delta)
    {
        if (delta == 0)
            throw new BusinessRuleException("El ajuste de stock no puede ser 0.");

        bool updated = await productRepository.TryAdjustStockAsync(id, delta);

        if (!updated)
        {
            // El UPDATE no afectó filas: distingo si el producto no existe (404) o si no hay stock (409).
            Product product = await GetExistingProductAsync(id);
            throw new InsufficientStockException(id, product.Stock, delta);
        }

        return await GetByIdAsync(id);
    }

    private async Task<Product> GetExistingProductAsync(int id) =>
        await productRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"El producto {id} no existe.");

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await categoryRepository.ExistsAsync(categoryId))
            throw new BusinessRuleException($"La categoría {categoryId} no existe.");
    }
}

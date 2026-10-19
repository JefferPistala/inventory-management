using Products.Api.Dtos;
using Products.Api.Entities;
using Products.Api.Exceptions;
using Products.Api.Mappings;
using Products.Api.Repositories;

namespace Products.Api.Services;

// Reglas de negocio de productos. Se lee como "qué debe pasar", sin detalles
// de SQL (eso está en el Repository) ni de HTTP (eso está en el Controller).
public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository) : IProductService
{
    public async Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryParameters query)
    {
        // Regla: un rango de precios invertido no tiene sentido.
        if (query.MinPrice > query.MaxPrice)
            throw new BusinessRuleException("El precio mínimo no puede ser mayor que el precio máximo.");

        var (items, totalCount) = await productRepository.GetPagedAsync(query);

        return new PagedResult<ProductResponse>(
            items.Select(p => p.ToResponse()).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        Product product = await GetExistingProductAsync(id);
        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        await EnsureCategoryExistsAsync(request.CategoryId);

        // Stock = 0: el inventario inicial se registra con una COMPRA,
        // así toda unidad tiene una transacción que la explica.
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

        // Se vuelve a leer para traer la categoría (Include) y los valores
        // que generó la base (Id, RowVersion).
        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        Product product = await GetExistingProductAsync(id);

        if (product.CategoryId != request.CategoryId)
            await EnsureCategoryExistsAsync(request.CategoryId);

        // El stock NO se toca aquí: solo cambia con transacciones.
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.Price = request.Price;
        product.UpdatedAt = DateTime.UtcNow;

        // Si otro usuario editó el producto entre la lectura y este guardado,
        // RowVersion no coincide y EF lanza DbUpdateConcurrencyException (→ 409).
        await productRepository.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        Product product = await GetExistingProductAsync(id);

        // Soft delete: el historial de transacciones nunca queda huérfano.
        // Desde aquí el filtro global lo oculta de todas las consultas.
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
            // El Repository solo dijo "0 filas". El Service interpreta POR QUÉ,
            // para devolver el error correcto: 404 o 409.
            Product product = await GetExistingProductAsync(id);   // lanza 404 si no existe
            throw new InsufficientStockException(id, product.Stock, delta);
        }

        return await GetByIdAsync(id);
    }

    // ---------------------------------------------------------------
    // Métodos privados de apoyo: evitan repetir la misma validación.
    // ---------------------------------------------------------------

    private async Task<Product> GetExistingProductAsync(int id) =>
        await productRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"El producto {id} no existe.");

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await categoryRepository.ExistsAsync(categoryId))
            throw new BusinessRuleException($"La categoría {categoryId} no existe.");
    }
}

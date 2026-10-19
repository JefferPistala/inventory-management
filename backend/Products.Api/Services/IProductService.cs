using Products.Api.Dtos;

namespace Products.Api.Services;

// Contrato de la lógica de negocio de productos.
// Recibe y devuelve DTOs: el Controller nunca ve las entidades.
public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryParameters query);
    Task<ProductResponse> GetByIdAsync(int id);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
    Task<ProductResponse> AdjustStockAsync(int id, int delta);
}

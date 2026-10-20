using Products.Api.Dtos;

namespace Products.Api.Services;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryParameters query);
    Task<ProductResponse> GetByIdAsync(int id, bool includeInactive = false);
    Task<ProductResponse> CreateAsync(CreateProductRequest request);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
    Task DeleteAsync(int id);
    Task<ProductResponse> AdjustStockAsync(int id, int delta);
}

using Products.Api.Dtos;

namespace Products.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
}

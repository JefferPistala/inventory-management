using Products.Api.Dtos;
using Products.Api.Mappings;
using Products.Api.Repositories;

namespace Products.Api.Services;

// Aunque hoy solo delega, mantiene la misma estructura por capas que
// productos: si mañana aparece una regla de categorías, ya tiene su lugar.
public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories.Select(c => c.ToResponse()).ToList();
    }
}

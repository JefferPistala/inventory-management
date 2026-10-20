using Products.Api.Entities;

namespace Products.Api.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<bool> ExistsAsync(int id);
}

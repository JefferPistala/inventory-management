using Products.Api.Entities;

namespace Products.Api.Repositories;

// Categories es un catálogo de solo lectura: no hay crear/editar/eliminar.
public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<bool> ExistsAsync(int id);
}

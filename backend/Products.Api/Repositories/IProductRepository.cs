using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Repositories;

// Contrato de acceso a datos de productos.
// El Service depende de esta INTERFAZ, no de EF Core: facilita las pruebas
// (se puede reemplazar por un repositorio falso) y aísla la base de datos.
public interface IProductRepository
{
    // Listado con filtros dinámicos y paginación: devuelve la página + el total.
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(ProductQueryParameters query);

    // Devuelve null si no existe. Por defecto los eliminados (soft delete) no se
    // encuentran; con includeInactive = true sí, para consultar su historial.
    // Decidir qué hacer con el null es responsabilidad del Service.
    Task<Product?> GetByIdAsync(int id, bool includeInactive = false);
    Task<bool> ExistsAsync(int id);

    Task AddAsync(Product product);

    // Guarda los cambios hechos sobre una entidad ya cargada.
    Task SaveChangesAsync();

    // UPDATE atómico del stock. true = se actualizó; false = no se cumplió la
    // condición (no existe o el stock no alcanza). Informa el HECHO técnico,
    // no lanza excepciones de negocio.
    Task<bool> TryAdjustStockAsync(int productId, int delta);
}

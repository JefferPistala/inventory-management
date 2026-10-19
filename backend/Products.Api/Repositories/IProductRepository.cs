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

    // Devuelve null si no existe (o está eliminado). Decidir qué hacer con
    // ese null es responsabilidad del Service, no del Repository.
    Task<Product?> GetByIdAsync(int id);

    Task<bool> ExistsAsync(int id);

    Task AddAsync(Product product);

    // Guarda los cambios hechos sobre una entidad ya cargada.
    Task SaveChangesAsync();

    // UPDATE atómico del stock. true = se actualizó; false = no se cumplió la
    // condición (no existe o el stock no alcanza). Informa el HECHO técnico,
    // no lanza excepciones de negocio.
    Task<bool> TryAdjustStockAsync(int productId, int delta);
}

using Microsoft.EntityFrameworkCore;
using Products.Api.Entities;

namespace Products.Api.Data;

// El DbContext es la "puerta" de EF Core hacia InventoryProductsDb.
// Regla: aquí solo van los DbSet y la configuración del modelo, nunca lógica.
// Quien lo usa es la capa Repository.
public class ProductsDbContext(DbContextOptions<ProductsDbContext> options) : DbContext(options)
{
    // Cada DbSet representa una tabla que se puede consultar con LINQ.
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Busca en este proyecto todas las clases IEntityTypeConfiguration<T>
        // (carpeta Data/Configurations) y las aplica. Así cada entidad tiene
        // su configuración en un archivo propio en vez de un método gigante.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductsDbContext).Assembly);
    }
}

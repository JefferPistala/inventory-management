using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Api.Entities;

namespace Products.Api.Data.Configurations;

// Mapeo de la entidad Product a la tabla Products creada por database.sql.
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        // Las longitudes coinciden con las columnas NVARCHAR del script.
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        // DECIMAL(18,2). Sin esto EF avisa que podría truncar decimales.
        builder.Property(p => p.Price).HasPrecision(18, 2);

        // Le indica a EF que es un ROWVERSION: lo genera SQL Server y EF lo
        // agrega al WHERE de cada UPDATE para detectar ediciones simultáneas.
        builder.Property(p => p.RowVersion).IsRowVersion();

        // Relación N:1 con Category usando la FK CategoryId (misma base).
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        // Filtro global de soft delete: todas las consultas sobre Products
        // agregan "WHERE IsActive = 1" automáticamente. Así un producto
        // "eliminado" desaparece en todos lados sin repetir el Where.
        builder.HasQueryFilter(p => p.IsActive);
    }
}

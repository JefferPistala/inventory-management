using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Api.Entities;

namespace Products.Api.Data.Configurations;

// La base la crea database.sql (sin migraciones). Esta clase solo le
// "enseña" a EF cómo es la tabla que ya existe.
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)   // coincide con NVARCHAR(100)
            .IsRequired();
    }
}

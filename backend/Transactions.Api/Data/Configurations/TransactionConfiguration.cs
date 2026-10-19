using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Api.Entities;

namespace Transactions.Api.Data.Configurations;

// Mapeo de la entidad Transaction a la tabla que crea database.sql.
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        // La propiedad se llama Type, pero la columna es TransactionType.
        // Convierto el enum al texto que exige el CHECK de la tabla
        // ('PURCHASE' o 'SALE') y de vuelta al leer.
        builder.Property(t => t.Type)
            .HasColumnName("TransactionType")
            .HasMaxLength(10)
            .HasConversion(
                type => type == TransactionType.Purchase ? "PURCHASE" : "SALE",
                value => value == "PURCHASE" ? TransactionType.Purchase : TransactionType.Sale);

        builder.Property(t => t.UnitPrice).HasPrecision(18, 2);

        // Le aviso a EF que TotalPrice lo calcula la base: así nunca intenta
        // escribirlo y lo vuelve a leer después de cada INSERT o UPDATE.
        builder.Property(t => t.TotalPrice)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

        builder.Property(t => t.Detail).HasMaxLength(500);
    }
}

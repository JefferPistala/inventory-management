using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transactions.Api.Entities;

namespace Transactions.Api.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        // El CHECK de la tabla solo acepta 'PURCHASE' o 'SALE'.
        builder.Property(t => t.Type)
            .HasColumnName("TransactionType")
            .HasMaxLength(10)
            .HasConversion(
                type => type == TransactionType.Purchase ? "PURCHASE" : "SALE",
                value => value == "PURCHASE" ? TransactionType.Purchase : TransactionType.Sale);

        builder.Property(t => t.UnitPrice).HasPrecision(18, 2);

        // Columna calculada en la base: EF nunca la escribe.
        builder.Property(t => t.TotalPrice)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("[Quantity] * [UnitPrice]", stored: true);

        builder.Property(t => t.Detail).HasMaxLength(500);
    }
}

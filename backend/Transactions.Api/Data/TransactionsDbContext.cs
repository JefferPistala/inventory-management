using Microsoft.EntityFrameworkCore;
using Transactions.Api.Entities;

namespace Transactions.Api.Data;

// DbContext de InventoryTransactionsDb. Este servicio solo conoce su propia base:
// los datos de productos los pide por HTTP al servicio de Productos.
public class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplico las configuraciones de la carpeta Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);
    }
}

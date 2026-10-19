using Transactions.Api.Dtos;
using Transactions.Api.Entities;

namespace Transactions.Api.Repositories;

// Contrato de acceso a datos de transacciones.
public interface ITransactionRepository
{
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionQueryParameters query);

    // Devuelvo null si no existe; el Service decide qué hacer.
    Task<Transaction?> GetByIdAsync(int id);

    Task AddAsync(Transaction transaction);

    Task SaveChangesAsync();

    Task DeleteAsync(Transaction transaction);
}

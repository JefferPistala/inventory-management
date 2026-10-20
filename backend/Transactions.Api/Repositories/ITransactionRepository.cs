using Transactions.Api.Dtos;
using Transactions.Api.Entities;

namespace Transactions.Api.Repositories;

public interface ITransactionRepository
{
    Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionQueryParameters query);

    Task<Transaction?> GetByIdAsync(int id);

    Task AddAsync(Transaction transaction);

    Task SaveChangesAsync();

    Task DeleteAsync(Transaction transaction);
}

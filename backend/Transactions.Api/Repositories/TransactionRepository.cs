using Microsoft.EntityFrameworkCore;
using Transactions.Api.Data;
using Transactions.Api.Dtos;
using Transactions.Api.Entities;

namespace Transactions.Api.Repositories;

public class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionQueryParameters query)
    {
        IQueryable<Transaction> transactions = context.Transactions.AsNoTracking();

        if (query.ProductId.HasValue)
            transactions = transactions.Where(t => t.ProductId == query.ProductId.Value);

        if (query.Type.HasValue)
            transactions = transactions.Where(t => t.Type == query.Type.Value);
        // Los filtros llegan en hora local y la base guarda UTC: convierto el inicio de cada día antes de comparar.
        if (query.From.HasValue)
        {
            DateTime startUtc = query.From.Value.Date.ToUniversalTime();
            transactions = transactions.Where(t => t.TransactionDate >= startUtc);
        }

        // "To" incluye el día completo.
        if (query.To.HasValue)
        {
            DateTime endExclusiveUtc = query.To.Value.Date.AddDays(1).ToUniversalTime();
            transactions = transactions.Where(t => t.TransactionDate < endExclusiveUtc);
        }

        int totalCount = await transactions.CountAsync();

        List<Transaction> items = await transactions
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<Transaction?> GetByIdAsync(int id) =>
        context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(Transaction transaction)
    {
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();

    // Eliminación física: el Service ya revirtió el stock antes de llamar aquí.
    public async Task DeleteAsync(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
    }
}

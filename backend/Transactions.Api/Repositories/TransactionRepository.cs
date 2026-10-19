using Microsoft.EntityFrameworkCore;
using Transactions.Api.Data;
using Transactions.Api.Dtos;
using Transactions.Api.Entities;

namespace Transactions.Api.Repositories;

// Aquí están todas las consultas a la tabla Transactions.
public class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public async Task<(IReadOnlyList<Transaction> Items, int TotalCount)> GetPagedAsync(TransactionQueryParameters query)
    {
        // Armo la consulta por partes; recién se ejecuta en CountAsync y ToListAsync.
        IQueryable<Transaction> transactions = context.Transactions.AsNoTracking();

        if (query.ProductId.HasValue)
            transactions = transactions.Where(t => t.ProductId == query.ProductId.Value);

        if (query.Type.HasValue)
            transactions = transactions.Where(t => t.Type == query.Type.Value);

        if (query.From.HasValue)
            transactions = transactions.Where(t => t.TransactionDate >= query.From.Value.Date);

        // Tomo "To" como el día completo: todo lo que sea antes del día siguiente.
        // Si comparara con "<= To", perdería las transacciones de ese día después de las 00:00.
        if (query.To.HasValue)
        {
            DateTime endExclusive = query.To.Value.Date.AddDays(1);
            transactions = transactions.Where(t => t.TransactionDate < endExclusive);
        }

        int totalCount = await transactions.CountAsync();

        // Las más recientes primero (lo natural en un historial).
        // Desempato por Id para que la paginación sea estable.
        List<Transaction> items = await transactions
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Con tracking: el Service puede modificarla y luego guardar.
    public Task<Transaction?> GetByIdAsync(int id) =>
        context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(Transaction transaction)
    {
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();

    // Las transacciones sí se eliminan físicamente: el stock se revierte antes
    // en el Service, así que el inventario queda consistente.
    public async Task DeleteAsync(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
    }
}

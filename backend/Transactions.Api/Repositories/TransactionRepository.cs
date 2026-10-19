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
        // Las fechas del filtro llegan como días ("2026-10-19") en la hora local del
        // servidor, pero en la base guardo UTC. Convierto el inicio de cada día a UTC
        // antes de comparar; si no, en la noche (Ecuador es UTC-5) una venta de hoy
        // quedaría registrada como "mañana" y no aparecería en el filtro.
        if (query.From.HasValue)
        {
            DateTime startUtc = query.From.Value.Date.ToUniversalTime();
            transactions = transactions.Where(t => t.TransactionDate >= startUtc);
        }

        // Tomo "To" como el día completo: todo lo anterior al inicio del día siguiente.
        if (query.To.HasValue)
        {
            DateTime endExclusiveUtc = query.To.Value.Date.AddDays(1).ToUniversalTime();
            transactions = transactions.Where(t => t.TransactionDate < endExclusiveUtc);
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

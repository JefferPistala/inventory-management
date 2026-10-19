namespace Transactions.Api.Dtos;

// Misma forma que el PagedResult de Productos. No lo comparto en una librería
// común a propósito: cada microservicio es dueño de su código y puede
// evolucionar sin obligar al otro a cambiar.
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

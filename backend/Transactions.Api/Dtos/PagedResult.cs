namespace Transactions.Api.Dtos;

// Duplicado a propósito: cada microservicio es dueño de sus contratos.
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

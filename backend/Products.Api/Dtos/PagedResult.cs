namespace Products.Api.Dtos;

// Respuesta genérica para cualquier listado paginado.
// El frontend necesita TotalCount y TotalPages para dibujar el paginador.
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    // Propiedad calculada: 23 registros con páginas de 10 → 3 páginas.
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

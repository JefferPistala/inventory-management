using System.ComponentModel.DataAnnotations;
using Transactions.Api.Entities;

namespace Transactions.Api.Dtos;

// Filtros dinámicos del listado e historial. Todos son opcionales:
// GET /api/transactions?productId=1&type=Sale&from=2026-10-01&to=2026-10-31&page=1&pageSize=10
public class TransactionQueryParameters
{
    public int? ProductId { get; set; }

    // Llega por nombre: type=Purchase o type=Sale.
    [EnumDataType(typeof(TransactionType), ErrorMessage = "El tipo debe ser Purchase o Sale.")]
    public TransactionType? Type { get; set; }

    // Rango de fechas. "To" lo trato como día completo en el Repository.
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 10;
}

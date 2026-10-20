using System.ComponentModel.DataAnnotations;
using Transactions.Api.Entities;

namespace Transactions.Api.Dtos;

public class TransactionQueryParameters
{
    public int? ProductId { get; set; }

    [EnumDataType(typeof(TransactionType), ErrorMessage = "El tipo debe ser Purchase o Sale.")]
    public TransactionType? Type { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 10;
}

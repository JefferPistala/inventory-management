using System.ComponentModel.DataAnnotations;
using Transactions.Api.Entities;

namespace Transactions.Api.Dtos;

// Sin TotalPrice: lo calcula la base de datos.
public record CreateTransactionRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    int ProductId,

    [EnumDataType(typeof(TransactionType), ErrorMessage = "El tipo debe ser Purchase o Sale.")]
    TransactionType Type,

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    int Quantity,

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    decimal UnitPrice,

    [MaxLength(500, ErrorMessage = "El detalle no puede superar 500 caracteres.")]
    string? Detail,

    DateTime? TransactionDate);

// Sin ProductId: el producto de una transacción no se puede cambiar.
public record UpdateTransactionRequest(
    [EnumDataType(typeof(TransactionType), ErrorMessage = "El tipo debe ser Purchase o Sale.")]
    TransactionType Type,

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    int Quantity,

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    decimal UnitPrice,

    [MaxLength(500, ErrorMessage = "El detalle no puede superar 500 caracteres.")]
    string? Detail,

    DateTime? TransactionDate);

public record TransactionResponse(
    int Id,
    DateTime TransactionDate,
    TransactionType Type,
    int ProductId,
    string ProductName,
    bool ProductIsActive,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Detail,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductHistoryResponse(
    int ProductId,
    string ProductName,
    int CurrentStock,
    bool IsActive,
    PagedResult<TransactionResponse> Transactions);

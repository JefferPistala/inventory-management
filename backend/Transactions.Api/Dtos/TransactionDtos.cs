using System.ComponentModel.DataAnnotations;
using Transactions.Api.Entities;

namespace Transactions.Api.Dtos;

// =====================================================================
// DTOs de transacciones. Con estas validaciones, .NET responde 400
// automáticamente antes de que la petición llegue al Service.
// =====================================================================

// Para registrar una compra o una venta.
// No incluyo TotalPrice: lo calcula la base, así nadie puede enviar un total falso.
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

    // Opcional: si no la envían, uso la fecha y hora actual.
    DateTime? TransactionDate);

// Para editar. No tiene ProductId a propósito: el producto de una transacción
// no se puede cambiar. Si se equivocaron de producto, se elimina y se crea otra.
public record UpdateTransactionRequest(
    [EnumDataType(typeof(TransactionType), ErrorMessage = "El tipo debe ser Purchase o Sale.")]
    TransactionType Type,

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    int Quantity,

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    decimal UnitPrice,

    [MaxLength(500, ErrorMessage = "El detalle no puede superar 500 caracteres.")]
    string? Detail,

    // Opcional: si no la envían, conservo la fecha original.
    DateTime? TransactionDate);

// Lo que devuelvo. Incluyo el nombre del producto (lo pido a Productos)
// para que el frontend pueda mostrarlo en la tabla sin otra llamada.
public record TransactionResponse(
    int Id,
    DateTime TransactionDate,
    TransactionType Type,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Detail,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

// Historial de un producto: sus datos actuales (nombre y stock)
//  más sus transacciones paginadas y filtradas.
public record ProductHistoryResponse(
    int ProductId,
    string ProductName,
    int CurrentStock,
    PagedResult<TransactionResponse> Transactions);

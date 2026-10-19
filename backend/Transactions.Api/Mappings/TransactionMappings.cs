using Transactions.Api.Dtos;
using Transactions.Api.Entities;

namespace Transactions.Api.Mappings;

// Convierto la entidad en el DTO de respuesta en un solo lugar.
// El nombre del producto no está en mi base, por eso lo recibo como parámetro.
public static class TransactionMappings
{
    public static TransactionResponse ToResponse(
        this Transaction transaction, string productName, bool productIsActive) => new(
        transaction.Id,
        transaction.TransactionDate,
        transaction.Type,
        transaction.ProductId,
        productName,
        productIsActive,
        transaction.Quantity,
        transaction.UnitPrice,
        transaction.TotalPrice,
        transaction.Detail,
        transaction.CreatedAt,
        transaction.UpdatedAt);
}

namespace Transactions.Api.Entities;

public class Transaction
{
    public int Id { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public TransactionType Type { get; set; }

    // Sin FK ni navegación: el producto vive en la base de otro servicio.
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    // Precio del momento: el historial no cambia si luego se modifica el precio del producto.
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; private set; }

    public string? Detail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

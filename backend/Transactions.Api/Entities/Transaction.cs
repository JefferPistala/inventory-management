namespace Transactions.Api.Entities;

// Esta clase representa una fila de la tabla Transactions.
public class Transaction
{
    public int Id { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // En la tabla la columna se llama TransactionType; aquí la llamo Type
    // porque transaction.TransactionType se lee redundante.
    public TransactionType Type { get; set; }

    // Referencia lógica al producto: no tengo FK ni navegación porque el
    // producto vive en otra base de datos (la del servicio de Productos).
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    // Guardo una copia del precio del momento, así el historial no cambia
    // si después se modifica el precio del producto.
    public decimal UnitPrice { get; set; }

    // La calcula SQL Server (columna calculada). Dejo el setter privado para
    // que nadie lo asigne desde el código; EF igual puede leerlo.
    public decimal TotalPrice { get; private set; }

    public string? Detail { get; set; }

    // Lo inicializo aquí porque EF envía todas las columnas al insertar
    // y el DEFAULT de la base no llegaría a aplicarse.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

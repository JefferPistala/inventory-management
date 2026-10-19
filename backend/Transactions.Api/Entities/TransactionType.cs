namespace Transactions.Api.Entities;

// Tipos de transacción. Empiezo en 1 a propósito: si alguien no envía el tipo,
// llega el valor por defecto 0, que no existe en el enum, y la validación lo
// rechaza en lugar de asumir una compra.
// En la base se guardan como 'PURCHASE' y 'SALE' (lo convierto en la configuración).
public enum TransactionType
{
    Purchase = 1,
    Sale = 2
}

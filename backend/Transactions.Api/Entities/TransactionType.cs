namespace Transactions.Api.Entities;

// Empieza en 1: si el tipo no llega, el 0 no es válido y la validación lo rechaza.
public enum TransactionType
{
    Purchase = 1,
    Sale = 2
}

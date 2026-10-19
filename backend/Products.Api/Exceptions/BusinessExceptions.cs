namespace Products.Api.Exceptions;

// =====================================================================
// Excepciones de negocio propias. El Service las lanza y el manejador
// global (Middleware/GlobalExceptionHandler) las traduce a códigos HTTP.
// Así los controllers no necesitan try/catch.
// =====================================================================

// → 404: el recurso pedido no existe (o fue eliminado con soft delete).
public class NotFoundException(string message) : Exception(message);

// → 400: la petición tiene buen formato pero rompe una regla simple
// (por ejemplo, una categoría que no existe o un ajuste de stock en 0).
public class BusinessRuleException(string message) : Exception(message);

// → 409: la petición es válida, pero choca con el estado actual del stock.
public class InsufficientStockException(int productId, int currentStock, int delta)
    : Exception($"Stock insuficiente para el producto {productId}: " +
                $"stock actual {currentStock}, se intentaron descontar {Math.Abs(delta)} unidades.")
{
    public int ProductId { get; } = productId;
    public int CurrentStock { get; } = currentStock;
}

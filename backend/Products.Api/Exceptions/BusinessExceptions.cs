namespace Products.Api.Exceptions;

public class NotFoundException(string message) : Exception(message);

public class BusinessRuleException(string message) : Exception(message);

public class InsufficientStockException(int productId, int currentStock, int delta)
    : Exception($"Stock insuficiente para el producto {productId}: " +
                $"stock actual {currentStock}, se intentaron descontar {Math.Abs(delta)} unidades.")
{
    public int ProductId { get; } = productId;
    public int CurrentStock { get; } = currentStock;
}

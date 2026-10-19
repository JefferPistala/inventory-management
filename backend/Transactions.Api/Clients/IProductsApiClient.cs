namespace Transactions.Api.Clients;

// Contrato para hablar con el microservicio de Productos.
// El Service usa esta interfaz y no sabe que detrás hay HTTP: eso me permite
// cambiar a gRPC o simular el cliente en pruebas sin tocar la lógica de negocio.
public interface IProductsApiClient
{
    // Incluye productos eliminados (IsActive = false).
    // Devuelvo null si el producto no existe (404 de Productos).
    Task<ProductInfo?> GetProductAsync(int productId);

    // Ajusto el stock (delta positivo suma, negativo resta).
    // Si Productos lo rechaza, lanzo la excepción que corresponde.
    Task AdjustStockAsync(int productId, int delta);
}

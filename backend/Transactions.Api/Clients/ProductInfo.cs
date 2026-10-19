namespace Transactions.Api.Clients;

// Solo los campos que necesito de la respuesta de Productos.
// Al deserializar, el JSON trae más propiedades, pero las ignoro.
// IsActive = false cuando el producto fue eliminado (soft delete).
public record ProductInfo(int Id, string Name, decimal Price, int Stock, bool IsActive);

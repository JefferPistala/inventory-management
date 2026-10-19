namespace Transactions.Api.Clients;

// Solo los campos que necesito de la respuesta de Productos.
// Al deserializar, el JSON trae más propiedades, pero las ignoro.
public record ProductInfo(int Id, string Name, decimal Price, int Stock);

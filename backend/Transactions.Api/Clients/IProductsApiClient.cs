namespace Transactions.Api.Clients;

public interface IProductsApiClient
{
    Task<ProductInfo?> GetProductAsync(int productId);

    Task AdjustStockAsync(int productId, int delta);
}

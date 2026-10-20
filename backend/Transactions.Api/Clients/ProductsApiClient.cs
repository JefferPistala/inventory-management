using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.Exceptions;

namespace Transactions.Api.Clients;

public class ProductsApiClient(HttpClient httpClient) : IProductsApiClient
{
    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        // includeInactive: el historial necesita el nombre aunque el producto esté eliminado.
        HttpResponseMessage response = await SendAsync(
            () => httpClient.GetAsync($"api/products/{productId}?includeInactive=true"));

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new ExternalServiceException(
                $"El servicio de productos respondió {(int)response.StatusCode} al consultar el producto {productId}.");

        return await response.Content.ReadFromJsonAsync<ProductInfo>();
    }

    public async Task AdjustStockAsync(int productId, int delta)
    {
        HttpResponseMessage response = await SendAsync(
            () => httpClient.PatchAsJsonAsync($"api/products/{productId}/stock", new { delta }));

        if (response.IsSuccessStatusCode)
            return;

        string detail = await ReadProblemDetailAsync(response);

        throw response.StatusCode switch
        {
            HttpStatusCode.NotFound => new NotFoundException(detail),
            HttpStatusCode.Conflict => new InsufficientStockException(detail),
            HttpStatusCode.BadRequest => new BusinessRuleException(detail),
            _ => new ExternalServiceException(
                $"El servicio de productos respondió {(int)response.StatusCode} al ajustar el stock.")
        };
    }

    // Una caída o un timeout de Products.Api se convierte en 503.
    private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ExternalServiceException("No se pudo comunicar con el servicio de productos.", ex);
        }
    }

    private static async Task<string> ReadProblemDetailAsync(HttpResponseMessage response)
    {
        const string fallback = "El servicio de productos rechazó la operación.";
        try
        {
            ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            return problem?.Detail ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }
}

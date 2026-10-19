using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Transactions.Api.Exceptions;

namespace Transactions.Api.Clients;

// Cliente HTTP tipado hacia Products.Api (comunicación síncrona por REST).
// El HttpClient me lo entrega IHttpClientFactory (ver Program.cs) con la URL
// base ya configurada; nunca hago "new HttpClient()" porque eso agota los sockets.
public class ProductsApiClient(HttpClient httpClient) : IProductsApiClient
{
    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        HttpResponseMessage response = await SendAsync(
            () => httpClient.GetAsync($"api/products/{productId}"));

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

        // Productos responde con ProblemDetails; reutilizo su "detail" para que
        // el usuario reciba el mismo mensaje claro (por ejemplo, cuánto stock queda).
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

    // Centralizo el manejo de fallas de red: si Productos está caído o no
    // responde a tiempo, lo convierto en ExternalServiceException (→ 503).
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
            // Si la respuesta no es un ProblemDetails válido, uso un mensaje genérico.
            return fallback;
        }
    }
}

using Transactions.Api.Clients;
using Transactions.Api.Dtos;
using Transactions.Api.Entities;
using Transactions.Api.Exceptions;
using Transactions.Api.Mappings;
using Transactions.Api.Repositories;

namespace Transactions.Api.Services;

// Lógica de negocio de transacciones. Aqui esta la parte mas delicada del sistema
// mantener el stock consistente entre DOS bases de datos de DOS servicios.
//
// No existe una transacción SQL que abarque ambas bases, así que uso una
// Saga mínima con compensación:
//   1. Pido a Productos que ajuste el stock (si no alcanza, responde 409 y corto aquí).
//   2. Guardo el cambio en mi base.
//   3. Si el paso 2 falla, compenso: aplico el delta inverso en Productos.
// El paso que más probablemente falla (stock insuficiente) va primero,
// así en el caso común no hay nada que deshacer.
public class TransactionService(
    ITransactionRepository transactionRepository,
    IProductsApiClient productsClient,
    ILogger<TransactionService> logger) : ITransactionService
{
    private const string UnavailableProductName = "Producto no disponible";

    public async Task<PagedResult<TransactionResponse>> GetPagedAsync(TransactionQueryParameters query)
    {
        ValidateDateRange(query);

        var (items, totalCount) = await transactionRepository.GetPagedAsync(query);

        Dictionary<int, string> productNames = await GetProductNamesAsync(items.Select(t => t.ProductId));

        return new PagedResult<TransactionResponse>(
            items.Select(t => t.ToResponse(productNames.GetValueOrDefault(t.ProductId, UnavailableProductName))).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<ProductHistoryResponse> GetProductHistoryAsync(int productId, TransactionQueryParameters query)
    {
        ValidateDateRange(query);

        // Primero obtengo el nombre y el stock actual del producto (una sola llamada).
        ProductInfo product = await productsClient.GetProductAsync(productId)
            ?? throw new NotFoundException($"El producto {productId} no existe.");

        // Fuerzo el filtro por producto: en el historial, el producto lo define la ruta.
        query.ProductId = productId;
        var (items, totalCount) = await transactionRepository.GetPagedAsync(query);

        var page = new PagedResult<TransactionResponse>(
            items.Select(t => t.ToResponse(product.Name)).ToList(),
            query.Page,
            query.PageSize,
            totalCount);

        return new ProductHistoryResponse(product.Id, product.Name, product.Stock, page);
    }

    public async Task<TransactionResponse> GetByIdAsync(int id)
    {
        Transaction transaction = await GetExistingTransactionAsync(id);
        string productName = await GetProductNameAsync(transaction.ProductId);
        return transaction.ToResponse(productName);
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request)
    {
        DateTime transactionDate = ValidateTransactionDate(request.TransactionDate ?? DateTime.UtcNow);

        // Al crear no hay transacción anterior: el delta es el efecto de la nueva.
        int delta = GetStockEffect(request.Type, request.Quantity);

        // Paso 1: ajusto el stock. Si es una venta y no alcanza, Productos responde
        // 409 y aquí se corta todo; no guardé nada, así que no hay nada que deshacer.
        await productsClient.AdjustStockAsync(request.ProductId, delta);

        var transaction = new Transaction
        {
            TransactionDate = transactionDate,
            Type = request.Type,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Detail = request.Detail?.Trim()
        };

        try
        {
            // Paso 2: guardo la transacción en mi base.
            await transactionRepository.AddAsync(transaction);
        }
        catch
        {
            // Paso 3: el guardado falló, así que devuelvo el stock como estaba.
            await CompensateStockAsync(request.ProductId, delta);
            throw;   // relanzo: el usuario igual debe saber que la operación falló
        }

        return await GetByIdAsync(transaction.Id);
    }

    public async Task<TransactionResponse> UpdateAsync(int id, UpdateTransactionRequest request)
    {
        Transaction transaction = await GetExistingTransactionAsync(id);

        DateTime transactionDate = ValidateTransactionDate(request.TransactionDate ?? transaction.TransactionDate);

        // delta = efecto(nueva) − efecto(antigua).
        // Ejemplo: venta de 5 corregida a venta de 3 → (−3) − (−5) = +2 (vuelven 2 al stock).
        int delta = GetStockEffect(request.Type, request.Quantity)
                  - GetStockEffect(transaction.Type, transaction.Quantity);

        // Si solo cambió el precio, el detalle o la fecha, el stock no se mueve
        // y no llamo a Productos (además, Productos rechaza un ajuste de 0).
        if (delta != 0)
            await productsClient.AdjustStockAsync(transaction.ProductId, delta);

        transaction.Type = request.Type;
        transaction.Quantity = request.Quantity;
        transaction.UnitPrice = request.UnitPrice;
        transaction.Detail = request.Detail?.Trim();
        transaction.TransactionDate = transactionDate;
        transaction.UpdatedAt = DateTime.UtcNow;

        try
        {
            await transactionRepository.SaveChangesAsync();
        }
        catch
        {
            if (delta != 0)
                await CompensateStockAsync(transaction.ProductId, delta);
            throw;
        }

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        Transaction transaction = await GetExistingTransactionAsync(id);

        // Eliminar es deshacer el efecto que tuvo: delta = −efecto.
        // Eliminar una venta devuelve unidades; eliminar una compra las quita
        // (y Productos lo rechaza si esas unidades ya se vendieron).
        int delta = -GetStockEffect(transaction.Type, transaction.Quantity);

        await productsClient.AdjustStockAsync(transaction.ProductId, delta);

        try
        {
            await transactionRepository.DeleteAsync(transaction);
        }
        catch
        {
            await CompensateStockAsync(transaction.ProductId, delta);
            throw;
        }
    }

    // ---------------------------------------------------------------
    // Métodos de apoyo
    // ---------------------------------------------------------------

    // Único lugar donde se decide el signo: compra suma, venta resta.
    // Así crear, editar y eliminar usan exactamente la misma regla.
    private static int GetStockEffect(TransactionType type, int quantity) =>
        type == TransactionType.Purchase ? quantity : -quantity;

    // Compensación: aplico el delta inverso para dejar el stock como estaba.
    // Si incluso esto falla (por ejemplo, Productos se cayó justo ahora), no
    // puedo hacer más que registrarlo como crítico para revisarlo a mano.
    // En un sistema grande esto se resolvería con reintentos y el patrón Outbox.
    private async Task CompensateStockAsync(int productId, int delta)
    {
        try
        {
            await productsClient.AdjustStockAsync(productId, -delta);
            logger.LogWarning(
                "Se compensó el stock del producto {ProductId} con delta {Delta} por una falla al guardar.",
                productId, -delta);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "No se pudo compensar el stock del producto {ProductId} (delta {Delta}). Requiere revisión manual.",
                productId, -delta);
        }
    }

    private async Task<Transaction> GetExistingTransactionAsync(int id) =>
        await transactionRepository.GetByIdAsync(id)
        ?? throw new NotFoundException($"La transacción {id} no existe.");

    private async Task<string> GetProductNameAsync(int productId)
    {
        Dictionary<int, string> names = await GetProductNamesAsync([productId]);
        return names.GetValueOrDefault(productId, UnavailableProductName);
    }

    // Pido los nombres de los productos de la página en paralelo, una vez por
    // producto distinto (no una por fila). Si Productos no responde, el listado
    // igual se muestra con "Producto no disponible" en vez de fallar entero.
    private async Task<Dictionary<int, string>> GetProductNamesAsync(IEnumerable<int> productIds)
    {
        List<int> distinctIds = productIds.Distinct().ToList();
        try
        {
            ProductInfo?[] products = await Task.WhenAll(distinctIds.Select(productsClient.GetProductAsync));
            return products
                .Where(p => p is not null)
                .ToDictionary(p => p!.Id, p => p!.Name);
        }
        catch (ExternalServiceException ex)
        {
            logger.LogWarning(ex, "No se pudieron obtener los nombres de los productos.");
            return [];
        }
    }

    private static void ValidateDateRange(TransactionQueryParameters query)
    {
        if (query.From > query.To)
            throw new BusinessRuleException("La fecha inicial no puede ser mayor que la fecha final.");
    }

    // Una transacción no puede registrarse en el futuro.
    // Dejo un minuto de tolerancia por diferencias de reloj entre equipos.
    private static DateTime ValidateTransactionDate(DateTime date)
    {
        if (date > DateTime.UtcNow.AddMinutes(1))
            throw new BusinessRuleException("La fecha de la transacción no puede ser futura.");
        return date;
    }
}

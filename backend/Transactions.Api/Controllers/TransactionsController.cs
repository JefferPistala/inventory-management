using Microsoft.AspNetCore.Mvc;
using Transactions.Api.Dtos;
using Transactions.Api.Services;

namespace Transactions.Api.Controllers;

// En el controller solo recibo la petición, delego al Service y devuelvo
// el código HTTP. Las reglas de stock viven en el Service.
[ApiController]
[Route("api/[controller]")]
public class TransactionsController(ITransactionService transactionService) : ControllerBase
{
    // GET /api/transactions?productId=&type=&from=&to=&page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TransactionResponse>>> GetPaged([FromQuery] TransactionQueryParameters query) =>
        Ok(await transactionService.GetPagedAsync(query));

    // GET /api/transactions/history/3?type=Sale&from=2026-10-01&to=2026-10-31
    // Historial de un producto con su nombre y stock actual.
    [HttpGet("history/{productId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductHistoryResponse>> GetProductHistory(
        int productId, [FromQuery] TransactionQueryParameters query) =>
        Ok(await transactionService.GetProductHistoryAsync(productId, query));

    // GET /api/transactions/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(int id) =>
        Ok(await transactionService.GetByIdAsync(id));

    // POST /api/transactions → 201 Created
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TransactionResponse>> Create(CreateTransactionRequest request)
    {
        TransactionResponse created = await transactionService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/transactions/5 → recalcula el stock con la diferencia.
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TransactionResponse>> Update(int id, UpdateTransactionRequest request) =>
        Ok(await transactionService.UpdateAsync(id, request));

    // DELETE /api/transactions/5 → revierte su efecto en el stock.
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Delete(int id)
    {
        await transactionService.DeleteAsync(id);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using Products.Api.Dtos;
using Products.Api.Services;

namespace Products.Api.Controllers;

// [ApiController] activa la validación automática de los DTOs:
// si un [Required] o [Range] falla, responde 400 sin entrar al método.
// [Route("api/[controller]")] → "api/products" (nombre de la clase sin "Controller").
[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    // Regla de esta capa: recibir, delegar al Service y traducir a HTTP.
    // Ni una sola regla de negocio aquí.

    // GET /api/products?name=&categoryId=&minPrice=&maxPrice=&page=1&pageSize=10
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetPaged([FromQuery] ProductQueryParameters query) =>
        Ok(await productService.GetPagedAsync(query));

    // GET /api/products/5   (":int" hace que "/api/products/abc" no entre aquí)
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id) =>
        Ok(await productService.GetByIdAsync(id));

    // POST /api/products → 201 Created + cabecera Location con la URL del nuevo producto.
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        ProductResponse created = await productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/products/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request) =>
        Ok(await productService.UpdateAsync(id, request));

    // DELETE /api/products/5 → 204 No Content (soft delete).
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await productService.DeleteAsync(id);
        return NoContent();
    }

    // PATCH /api/products/5/stock   body: { "delta": -3 }
    // PATCH porque modifica UNA parte del recurso (el stock), no el producto entero.
    // Lo usa Transactions.Api al registrar, editar o eliminar transacciones.
    [HttpPatch("{id:int}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> AdjustStock(int id, AdjustStockRequest request) =>
        Ok(await productService.AdjustStockAsync(id, request.Delta));
}

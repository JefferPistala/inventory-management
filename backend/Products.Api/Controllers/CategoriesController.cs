using Microsoft.AspNetCore.Mvc;
using Products.Api.Dtos;
using Products.Api.Services;

namespace Products.Api.Controllers;

// Solo lectura: llena el <select> de categorías en el frontend.
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    // GET /api/categories
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll() =>
        Ok(await categoryService.GetAllAsync());
}

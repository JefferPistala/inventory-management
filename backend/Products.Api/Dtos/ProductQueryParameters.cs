using System.ComponentModel.DataAnnotations;

namespace Products.Api.Dtos;

// Filtros dinámicos del listado. Llegan como query string:
// GET /api/products?name=mouse&categoryId=1&minPrice=10&page=1&pageSize=10
// Todos los filtros son opcionales: el que no llega, no se aplica.
public class ProductQueryParameters
{
    // Búsqueda parcial por nombre (LIKE '%texto%').
    public string? Name { get; set; }

    public int? CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo no puede ser negativo.")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio máximo no puede ser negativo.")]
    public decimal? MaxPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor o igual a 1.")]
    public int Page { get; set; } = 1;

    // Límite de 100: evita que alguien pida un millón de filas de una vez.
    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 10;
}

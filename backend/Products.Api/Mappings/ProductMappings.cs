using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Mappings;

// Conversión Entity → DTO en un solo lugar.
// Son métodos de extensión: permiten escribir product.ToResponse().
// Se hace a mano (sin AutoMapper) porque son pocos campos y así se ve
// exactamente qué sale por la API.
public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.CategoryId,
        product.Category.Name,   // requiere haber cargado la categoría con Include
        product.ImageUrl,
        product.Price,
        product.Stock,
        product.CreatedAt,
        product.UpdatedAt);

    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name);
}

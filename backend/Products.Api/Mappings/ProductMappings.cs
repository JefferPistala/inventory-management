using Products.Api.Dtos;
using Products.Api.Entities;

namespace Products.Api.Mappings;
public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.CategoryId,
        product.Category.Name, 
        product.ImageUrl,
        product.Price,
        product.Stock,
        product.IsActive,
        product.CreatedAt,
        product.UpdatedAt);

    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name);
}

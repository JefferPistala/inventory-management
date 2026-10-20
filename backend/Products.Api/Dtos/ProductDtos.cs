using System.ComponentModel.DataAnnotations;

namespace Products.Api.Dtos;

// Sin Stock: el stock solo cambia con compras y ventas.
public record CreateProductRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    string Name,

    [MaxLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    string? Description,

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    int CategoryId,

    [MaxLength(500, ErrorMessage = "La URL de la imagen no puede superar 500 caracteres.")]
    [Url(ErrorMessage = "La imagen debe ser una URL válida.")]
    string? ImageUrl,

    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
    decimal Price);

public record UpdateProductRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
    string Name,

    [MaxLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    string? Description,

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
    int CategoryId,

    [MaxLength(500, ErrorMessage = "La URL de la imagen no puede superar 500 caracteres.")]
    [Url(ErrorMessage = "La imagen debe ser una URL válida.")]
    string? ImageUrl,

    [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
    decimal Price);


public record AdjustStockRequest(int Delta);


public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    string? ImageUrl,
    decimal Price,
    int Stock,
    bool IsActive,       //  false solo al consultar con includeInactive
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CategoryResponse(int Id, string Name);

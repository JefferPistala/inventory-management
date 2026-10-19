using System.ComponentModel.DataAnnotations;

namespace Products.Api.Dtos;

// Lo que se envía al CREAR un producto.
// No tiene Stock: un producto nuevo nace con 0 y solo cambia con compras/ventas.
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

// Lo que se envía al EDITAR. Tampoco tiene Stock: así es imposible
// modificarlo desde el formulario, aunque alguien manipule la petición.
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

// Uso interno de Transactions.Api: delta positivo = compra, negativo = venta.
public record AdjustStockRequest(int Delta);

// Lo que la API devuelve. Incluye el nombre de la categoría para que el
// frontend no tenga que hacer otra llamada. No expone RowVersion ni IsActive.
public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    string? ImageUrl,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CategoryResponse(int Id, string Name);

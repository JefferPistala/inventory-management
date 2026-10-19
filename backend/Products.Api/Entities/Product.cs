namespace Products.Api.Entities;

// Representa una fila de la tabla Products.
// Las propiedades siguen el mismo orden que las columnas del script SQL.
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // NVARCHAR(500) NULL → string? : el "?" dice que puede venir vacío.
    public string? Description { get; set; }

    // Columna FK (el número que se guarda en la tabla).
    public int CategoryId { get; set; }

    // Navegación: el objeto Category completo, para leer product.Category.Name.
    // "= null!" le dice al compilador que EF lo llenará (con Include).
    public Category Category { get; set; } = null!;

    // Guardamos la URL de la imagen, no el binario.
    public string? ImageUrl { get; set; }

    // DECIMAL(18,2): decimal en C#, nunca double/float (dinero exacto).
    public decimal Price { get; set; }

    // El stock SOLO cambia con compras/ventas (endpoint de ajuste atómico).
    // Un producto nuevo nace con 0 unidades.
    public int Stock { get; set; }

    // Soft delete. IMPORTANTE: se inicializa en true porque EF envía TODAS las
    // columnas al insertar; si quedara en false (valor por defecto de bool),
    // el DEFAULT 1 de SQL nunca se aplicaría y el producto nacería "eliminado".
    public bool IsActive { get; set; } = true;

    // Misma razón que IsActive: no dependemos del DEFAULT de la base.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // ROWVERSION → byte[]. SQL Server lo cambia en cada UPDATE.
    // EF lo usa para detectar si otro usuario editó la fila mientras tanto
    // (concurrencia optimista).
    public byte[] RowVersion { get; set; } = [];
}

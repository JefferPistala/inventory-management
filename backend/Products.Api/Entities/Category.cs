namespace Products.Api.Entities;

// Representa una fila de la tabla Categories (catálogo fijo, sin CRUD).
// La entidad refleja la TABLA; lo que viaja por la API son los DTOs.
public class Category
{
    // INT IDENTITY: lo genera SQL Server al insertar, nunca lo asignamos nosotros.
    public int Id { get; set; }

    // NVARCHAR(100) NOT NULL → string no anulable.
    // string.Empty evita la advertencia de "nullable" del compilador.
    public string Name { get; set; } = string.Empty;

    // Navegación inversa: los productos de esta categoría.
    // No es una columna; EF la llena a partir de la FK Products.CategoryId.
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

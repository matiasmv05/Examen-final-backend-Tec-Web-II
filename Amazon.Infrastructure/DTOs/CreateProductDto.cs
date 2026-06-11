namespace Amazon.infrastructure.DTOs
{
    /// <summary>
    /// DTO exclusivo para la creación de productos.
    /// No incluye Id (se genera automáticamente) ni SellerId (se extrae del token JWT).
    /// </summary>
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int stock { get; set; }
        public string? ImageUrl { get; set; }
    }
}

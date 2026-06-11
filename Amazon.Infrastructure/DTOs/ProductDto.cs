namespace Amazon.infrastructure.DTOs
{
    public class ProductDto
    {
        public int? Id { get; set; }

        public int SellerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int stock { get; set; }
        public string? ImageUrl { get; set; }
    }
}

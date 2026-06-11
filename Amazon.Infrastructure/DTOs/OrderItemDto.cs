namespace Amazon.infrastructure.DTOs
{
    public class OrderItemDto
    {
        public int? OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
        public string? ProductName { get; set; }
    }
}

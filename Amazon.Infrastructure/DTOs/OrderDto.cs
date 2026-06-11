namespace Amazon.infrastructure.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount => OrderItems.Sum(item => item.Subtotal);

    }
}

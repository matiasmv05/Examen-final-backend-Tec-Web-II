using System;

namespace Amazon.Infrastructure.DTOs
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

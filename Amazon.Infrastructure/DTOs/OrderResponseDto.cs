using Amazon.infrastructure.DTOs;
using System;
using System.Collections.Generic;

namespace Amazon.Infrastructure.DTOs
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}

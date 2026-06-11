using Amazon.Core.CustomEntities;
using System.Collections.Generic;

namespace Amazon.Infrastructure.DTOs
{
    public class CreateOrderRequest
    {
        public virtual List<OrderItemRequest> OrderItems { get; set; } = new();
    }
}

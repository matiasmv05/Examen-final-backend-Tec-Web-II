using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class CrearOrdenRequest
    {
        public int UserId { get; set; }
        public virtual List<OrderItemRequest> OrderItems { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class Order_Item : BaseEntity
    {
       // public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
        public decimal UnitPrice { get; set; }
        public virtual Order? order { get; set; }
        public virtual Product? Product { get; set; }
    }
}

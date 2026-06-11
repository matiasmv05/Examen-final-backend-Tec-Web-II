using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class Product:BaseEntity
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public int SellerId { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual User? Seller { get; set; }
        public virtual ICollection<Order_Item> OrderItems { get; set; } = new List<Order_Item>();
    }
}

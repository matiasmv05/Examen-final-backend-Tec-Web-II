using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class Order : BaseEntity
    {
        //public int Id { get; set; }
        public int UserId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; } = "Cart"; // Cart, Paid, Cancelled
        public DateTime? UpdatedAt { get; set; }= DateTime.Now; 
        public virtual ICollection<Order_Item> OrderItems { get; set; } = new List<Order_Item>();
        public virtual User? User { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}

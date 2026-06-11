using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class User : BaseEntity
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal? Billetera { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
         public bool IsActive { get; set; } = true; 
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

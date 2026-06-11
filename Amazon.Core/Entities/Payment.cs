using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class Payment:BaseEntity
    {
        //public int Id { get; set; }
        public int? OrderId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual Order? Order { get; set; } 
    }
}

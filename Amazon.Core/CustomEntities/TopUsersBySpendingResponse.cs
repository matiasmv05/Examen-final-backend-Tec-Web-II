using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class TopUsersBySpendingResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal TotalGastado { get; set; }
        public int TotalOrdenes { get; set; }
    }
}

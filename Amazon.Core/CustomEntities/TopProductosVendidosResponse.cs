using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class TopProductosVendidosResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int TotalVendido { get; set; }
        public decimal IngresosTotales { get; set; }
    }
}

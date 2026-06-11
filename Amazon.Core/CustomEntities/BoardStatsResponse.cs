using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class BoardStatsResponse
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosConAltoSaldo { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosSinStock { get; set; }
        public int TotalOrdenes { get; set; }
        public int OrdenesPagadas { get; set; }
        public int CarritosActivos { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal SaldoTotalUsuarios { get; set; }
    }
}

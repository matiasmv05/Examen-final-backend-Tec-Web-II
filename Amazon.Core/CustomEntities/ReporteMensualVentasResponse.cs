using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class ReporteMensualVentasResponse
    {
        public int Año { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public int OrdenesCompletadas { get; set; }
        public int ProductosVendidos { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal TicketPromedio { get; set; }
    }
}

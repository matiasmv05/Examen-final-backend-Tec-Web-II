using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    /// <summary>
    /// Filtra los parámetros de pagos
    /// </summary>
    /// <remarks>
    /// Esta clase permite filtrar pagos por diferentes criterios como orden asociada,
    /// estado del pago, monto total y fecha de creación. Incluye paginación automática.
    /// 
    /// Ejemplo de uso en URL:
    /// ?OrderId=1&Status=Completed&PageSize=20&PageNumber=1
    /// </remarks>
    public class PaymentQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Identificador único de la orden asociada al pago
        /// </summary>
        /// <example>1</example>
        [SwaggerSchema("Identificador único de la orden asociada al pago")]
        public int OrderId { get; set; }

        /// <summary>
        /// Estado actual del pago
        /// </summary>
        /// <example>Completed</example>
        [SwaggerSchema("Estado actual del pago")]
        public string Status { get; set; }

        /// <summary>
        /// Monto total del pago
        /// </summary>
        /// <example>1250.50</example>
        [SwaggerSchema("Monto total del pago")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Fecha de creación del pago
        /// </summary>
        /// <example>2025-11-19T21:04:02.24</example>
        [SwaggerSchema("Fecha de creación del pago")]
        public DateTime CreatedAt { get; set; }
    }
}

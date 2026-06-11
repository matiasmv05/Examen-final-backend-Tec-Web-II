using Amazon.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.Core.Enum;

namespace Amazon.Core.Entities
{
    public partial class Security : BaseEntity
    {
        public int UserId { get; set; }       // faltaba esto
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }     // ignorado en BD, solo tránsito
        public RoleType? Role { get; set; }
        public virtual User? User { get; set; }
    }
}
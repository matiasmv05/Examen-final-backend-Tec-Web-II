using Amazon.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.DTOs
{
    public class SecurityDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public RoleType? Role { get; set; }
}
}

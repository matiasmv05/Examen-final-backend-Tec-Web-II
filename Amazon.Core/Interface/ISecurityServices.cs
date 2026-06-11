using Amazon.Core.Entities;
using Amazon.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface ISecurityServices
    {
        Task<Security> GetLoginByCredentials(UserLogin login);

        Task RegisterUser(Security security);

        Task UpdateRoleAsync(int userId, RoleType role);
    }
}

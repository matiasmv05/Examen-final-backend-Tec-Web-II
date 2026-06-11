using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Exceptions;
using Amazon.Core.Interface;
using Amazon.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{
    public class SecurityRepository : BaseRepository<Security>, ISecurityRepository
    {
        public SecurityRepository(AmazonContext context, IDapperContext dapper) : base(context) { }

        public async Task<Security> GetLoginByCredentials(UserLogin login)
{
    return await _entities
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.User.Email == login.Email);
}

    public async Task UpdateRoleAsync(int userId, RoleType role)
{
    var security = await _entities
        .FirstOrDefaultAsync(x => x.UserId == userId);

    if (security == null)
        throw new BussinesException("Credenciales del usuario no encontradas", HttpStatusCode.NotFound);

    security.Role = role;
    await Update(security);
}
}
}

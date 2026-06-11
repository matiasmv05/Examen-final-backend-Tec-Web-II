using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class SecurityServices : ISecurityServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public SecurityServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Security> GetLoginByCredentials(UserLogin login)
        {
            return await _unitOfWork.SecurityRepository.GetLoginByCredentials(login);
        }

        public async Task RegisterUser(Security security)
{
    await _unitOfWork.BeginTransaccionAsync();
    try
    {
        var user = new User
        {
            Name = security.Name,
            Email = security.Email, // viene del SecurityDto
            IsActive = true,
            Billetera = 0
        };
        await _unitOfWork.UserRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(); // genera user.Id

        security.UserId = user.Id;
        await _unitOfWork.SecurityRepository.Add(security);

        await _unitOfWork.CommitAsync();
    }
    catch
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }
}
public async Task UpdateRoleAsync(int userId, RoleType role)
{
    await _unitOfWork.SecurityRepository.UpdateRoleAsync(userId, role);
    await _unitOfWork.SaveChangesAsync();
}
    }
}

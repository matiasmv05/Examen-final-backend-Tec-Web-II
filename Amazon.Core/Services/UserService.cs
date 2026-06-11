using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using Amazon.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class UserService : IUserService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public UserService(
             //IOrderRepository orderRepository,
             //IUserRepository userRepository,
             //IProductRepository productRepository,
             //IPaymentRepository paymentRepository)
             IUnitOfWork unitOfWork)
        {
            //_orderRepository = orderRepository;
            //_userRepository = userRepository;
            //_productRepository = productRepository;
            //_paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }


        /// //////////////////////////////////////////////////////////////////

        public async Task<ResponseData> GetAllUsers(UserQueryFilter filters)
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();

            if (filters.Name != null)
            {
                users = users.Where(x => x.Name == filters.Name);
            }

            if (filters.Email != null)
            {
                users = users.Where(x => x.Email == filters.Email);
            }

            
            var pagedOrders = PagedList<object>.Create(users, filters.PageNumber, filters.PageSize);
            if (pagedOrders.Any())
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Information", Description = "Registros de orders recuperados correctamente" } },
                    Pagination = pagedOrders,
                    StatusCode = HttpStatusCode.OK
                };
            }
            else
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Warning", Description = "No fue posible recuperar la cantidad de registros" } },
                    Pagination = pagedOrders,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
        }
        
            
        public async Task<User> GetByEmailAsync(string email)
        {
        return await _unitOfWork.UserRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
        return await _unitOfWork.UserRepository.GetAllAsync();
        }

        public async Task UpdateWalletAsync(int userId, decimal amount)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
        if (user == null || !user.IsActive)
        throw new BussinesException("Usuario no encontrado o ya inactivo", HttpStatusCode.NotFound);
    
         await _unitOfWork.UserRepository.UpdateWalletAsync(userId, amount);
         await _unitOfWork.SaveChangesAsync();

        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _unitOfWork.UserRepository.GetById(id);
        }

        public async Task AddAsync(User user)
        {
        await _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Update(User user)
        {
            await _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            var user = await _unitOfWork.UserRepository.GetById(id);
             if (user != null && user.IsActive)
            {
                await _unitOfWork.UserRepository.SoftDeleteAsync(id);
                 await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw new BussinesException("Usuario no encontrado o ya inactivo", HttpStatusCode.NotFound);
            }
    
        }

        public async Task<User> ActivarUsuario(int id)
        {
            var user = await _unitOfWork.UserRepository.GetById(id);
             if (user != null && !user.IsActive)
            {
                user.IsActive = true;
                await _unitOfWork.UserRepository.Update(user);
                 await _unitOfWork.SaveChangesAsync();
                 
                return user;
            }
            else
            {
                throw new BussinesException("Usuario no encontrado o ya activo", HttpStatusCode.NotFound);
            }
    
        }

}
}

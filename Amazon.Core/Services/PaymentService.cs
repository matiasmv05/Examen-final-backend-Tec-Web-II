using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Services
{
    public class PaymentService : IPaymentService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public PaymentService(
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

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _unitOfWork.PaymentRepository.GetAllAsync();
        }

        public async Task<ResponseData> GetAllPayments(PaymentQueryFilter filters)
        {
            var payments = await _unitOfWork.PaymentRepository.GetAllAsync();

            if (filters.OrderId > 0)
            {
                payments = payments.Where(x => x.OrderId == filters.OrderId);
            }

            if (filters.TotalAmount > 0)
            {
                payments = payments.Where(x => x.TotalAmount == filters.TotalAmount);
            }

            if (filters.Status != null)
            {
                payments = payments.Where(x => x.Status == filters.Status);
            }

            var pagedOrders = PagedList<object>.Create(payments, filters.PageNumber, filters.PageSize);
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
        public async Task<Payment> GetByIdAsync(int id)
        {
            return await _unitOfWork.PaymentRepository.GetById(id);
        }

        public async Task AddAsync(Payment payment)
        {
            await _unitOfWork.PaymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Update(Payment payment)
        {
            await _unitOfWork.PaymentRepository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task Delete(int id)
        {
            await _unitOfWork.PaymentRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();

        }

        public Task<Payment> GetByOrderIdAsync(int orderId)
        {
           return _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
        }
    }
}

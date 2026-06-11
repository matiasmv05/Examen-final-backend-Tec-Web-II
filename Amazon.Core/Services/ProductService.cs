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
    public class ProductService : IProductService
    {
        //private readonly IOrderRepository _orderRepository;
        //private readonly IUserRepository _userRepository;
        //private readonly IProductRepository _productRepository;
        //private readonly IPaymentRepository _paymentRepository;

        public readonly IUnitOfWork _unitOfWork;


        public ProductService(
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

        public async Task<ResponseData> GetAllProducts(ProductQueryFilter filters)
        {
            var products = await _unitOfWork.ProductRepository.GetAll();

            if (filters.Name != null)
            {
                products = products.Where(x => x.Name == filters.Name);
            }

            if (filters.SellerId > 0)
            {
                products = products.Where(x => x.SellerId == filters.SellerId);
            }

            if (filters.Category != null)
            {
                products = products.Where(x => x.Category == filters.Category);
            }

            if (filters.Price > 0)
            {
                products = products.Where(x => x.Price == filters.Price);
            }

            if (filters.Description != null)
            {
                products = products.Where(x => x.Description == filters.Description);
            }

            var pagedOrders = PagedList<object>.Create(products, filters.PageNumber, filters.PageSize);
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

        public async Task AddAsync(Product product)
        {
            await _unitOfWork.ProductRepository.Add(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
           await _unitOfWork.ProductRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _unitOfWork.ProductRepository.GetAll();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _unitOfWork.ProductRepository.GetByCategoryAsync(category);
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _unitOfWork.ProductRepository.GetById(id);
        }

        public Task<IEnumerable<Product>> GetBySellerAsync(int sellerId)
        {
            return _unitOfWork.ProductRepository.GetBySellerAsync(sellerId);
        }

        public async Task Update(Product product)
        {
            await _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

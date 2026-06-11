using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();

            // Product Mappings
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();

            // CreateProductDto → Product (sin Id ni SellerId, se asignan en el controlador)
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SellerId, opt => opt.Ignore());

            // Order Mappings 
            CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            // OrderItem Mappings 
            CreateMap<Order_Item, OrderItemDto>();
            CreateMap<OrderItemDto, Order_Item>();

            // Payment Mappings
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentDto, Payment>();

            CreateMap<Payment, PaymentResponseDto>();


            CreateMap<CrearOrdenRequest, Order>()
            .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItemRequest, Order_Item>();
            CreateMap<Security, SecurityDto>().ReverseMap();
            
            CreateMap<Security, RegisterResponseDto>();
            CreateMap<User, UserProfileDto>();

        }
    }
}


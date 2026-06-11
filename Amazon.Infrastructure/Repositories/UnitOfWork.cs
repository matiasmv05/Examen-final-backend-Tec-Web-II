﻿using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AmazonContext _context;
        public readonly ISecurityRepository _securityRepository;
        public readonly IOrderRepository? _orderRepository;
        public readonly IUserRepository? _userRepository;
        public readonly IPaymentRepository? _paymentRepository;
        public readonly IProductRepository? _productRepository;
        public readonly IOrderItemRepository? _orderItemRepository;
        public readonly IDapperContext _dapper;

        private IDbContextTransaction? _efTransaction;

        public UnitOfWork(AmazonContext context, IDapperContext dapper)
        {
            _context = context;
            _dapper = dapper;
        }

        public IOrderRepository OrderRepository=>
            _orderRepository ?? new OrderRepository(_context, _dapper);

        public IUserRepository UserRepository =>
            _userRepository ?? new UserRepository(_context, _dapper);
        public IPaymentRepository PaymentRepository =>
            _paymentRepository ?? new PaymentRepository(_context, _dapper);
        public IProductRepository ProductRepository =>
            _productRepository ?? new ProductRepository(_context, _dapper);
        public IOrderItemRepository OrderItemRepository =>
            _orderItemRepository ?? new OrderItemRepository(_context, _dapper);
        public ISecurityRepository SecurityRepository =>
            _securityRepository ?? new SecurityRepository(_context, _dapper);
        public void Dispose()
        {
            if (_context != null)
            {
                _efTransaction?.Dispose();
                _context.Dispose();
            }
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransaccionAsync()
        {
            if (_efTransaction == null)
            {
                _efTransaction = await _context.Database.BeginTransactionAsync();

                //Registrar coneccion/tx DapperContext
                var conn = _context.Database.GetDbConnection();
                var tx = _efTransaction.GetDbTransaction();
                _dapper.SetAmbientConnection(conn, tx);
            }
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_efTransaction != null)
                {
                    await _efTransaction.CommitAsync();
                    _efTransaction.Dispose();
                    _efTransaction = null;
                }
            }
            finally
            {
                _dapper.ClearAmbientConnection();
            }
        }

        public async Task RollbackAsync()
        {
            if (_efTransaction != null)
            {
                await _efTransaction.RollbackAsync();
                _efTransaction.Dispose();
                _efTransaction = null;
            }

            _dapper.ClearAmbientConnection();
        }

        public IDbConnection? GetDbConnection()
        {
            //Retornar la coneccion subyacente del DbContext
            return _context.Database.GetDbConnection();
        }

        public IDbTransaction? GetDbTransaction()
        {
            return _efTransaction?.GetDbTransaction();
        }
    }
}
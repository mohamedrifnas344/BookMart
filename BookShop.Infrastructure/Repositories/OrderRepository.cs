using BookShop.Application.Interfaces;
using BookShop.Domain.Entities;
using BookShop.Domain.Enums;
using BookShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddOrderAsync(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.Orders.CountAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(OrderStatus? status = null , string? search = null)
        {
            var query = _dbContext.Orders
                .Include(q => q.Items)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(o => o.FirstName.Contains(search) ||
                                    o.LastName.Contains(search));
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersByUserIdAsync(string userId , OrderStatus? status = null)
        {
            var query = _dbContext.Orders
                .Include(q => q.Items)
                .Where(x => x.AppUserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking().AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == status.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _dbContext.Orders
                .Include(q => q.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if (order == null)
            {
                return;
            }

            order.OrderStatus = status;
            if(order.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                if(status == OrderStatus.Delivered)
                {
                    order.PaymentStatus = PaymentStatus.Success;
                    order.PaymentDate = DateTime.UtcNow;
                }
                else
                {
                    order.PaymentStatus = PaymentStatus.Pending;
                    order.PaymentDate = null;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if(order == null)
            {
                return;
            }

            order.PaymentStatus = status;
            if(status == PaymentStatus.Success)
            {
                order.PaymentDate = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateStripeSessionDdAsync(Guid orderId, string sessionId)
        {
            var order = await _dbContext.Orders.FindAsync(orderId);
            if(order == null)
            {
                return;
            }

            order.SessionId = sessionId;
            await _dbContext.SaveChangesAsync();
        }
    }
}

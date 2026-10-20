using BookShop.Domain.Entities;
using BookShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllOrdersByUserIdAsync(string userId , OrderStatus? status = null);
        Task UpdateStripeSessionDdAsync(Guid orderId, string sessionId);
        Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<IEnumerable<Order>> GetAllOrdersAsync(OrderStatus? status = null , string? search = null);
        Task<int> CountAsync();
    }
}

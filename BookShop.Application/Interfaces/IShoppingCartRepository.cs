using BookShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.Interfaces
{
    public interface IShoppingCartRepository
    {
        Task AddGuestCartItemAsync(string guestId, Guid bookId, int quantity);
        Task AddUserCartItemAsync(string userId, Guid bookId, int quantity);
        Task MergeGuestCartAsync(string guestId, string userId);
        Task<ShoppingCart?> GetCartForGuestIdAsync(string guestId);
        Task<ShoppingCart?> GetCartForUserIdAsync(string userId);
        Task<bool> IncrementCartItemAsync(string? userId, string? guestId, Guid bookId);
        Task<bool> DecrementCartItemAsync(string? userId, string? guestId, Guid bookId);
        Task<bool> RemoveCartItemAsync(string? userId, string? guestId, Guid bookId);
        Task ClearCartAsync(string userId);
    }
}

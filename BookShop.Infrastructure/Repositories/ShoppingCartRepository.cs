using BookShop.Application.Interfaces;
using BookShop.Domain.Entities;
using BookShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Infrastructure.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ShoppingCartRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddGuestCartItemAsync(string guestId, Guid bookId, int quantity)
        {
            var cart = await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .FirstOrDefaultAsync(x => x.GuestId == guestId);
            if(cart == null)
            {
                cart = new ShoppingCart
                {
                    Id = Guid.NewGuid(),
                    GuestId = guestId,
                    Items = new List<ShoppingCartItem>()
                };
                _dbContext.ShoppingCarts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(b => b.BookId == bookId);
            if(existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    ShoppingCartId = cart.Id,
                    Quantity = quantity
                };
                _dbContext.ShoppingCartItems.Add(newItem);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddUserCartItemAsync(string userId, Guid bookId, int quantity)
        {
            var cart = await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    Id = Guid.NewGuid(),
                    AppUserId = userId,
                    Items = new List<ShoppingCartItem>()
                };
                _dbContext.ShoppingCarts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(b => b.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    ShoppingCartId = cart.Id,
                    Quantity = quantity
                };
                _dbContext.ShoppingCartItems.Add(newItem);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = await _dbContext.ShoppingCartItems
                .Where(x => x.ShoppingCart.AppUserId == userId)
                .ToListAsync();
            _dbContext.ShoppingCartItems.RemoveRange(items);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DecrementCartItemAsync(string? userId, string? guestId, Guid bookId)
        {
            var cartItem = await GetCartItemAsync(userId, guestId, bookId);
            if (cartItem == null)
            {
                return false;
            }

            if(cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                _dbContext.ShoppingCartItems.Remove(cartItem);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ShoppingCart?> GetCartForGuestIdAsync(string guestId)
        {
            return await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .ThenInclude(b => b.Book)
                .FirstOrDefaultAsync(x => x.GuestId == guestId);
        }

        public async Task<ShoppingCart?> GetCartForUserIdAsync(string userId)
        {
            return await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .ThenInclude(b => b.Book)
                .FirstOrDefaultAsync(x => x.AppUserId == userId);
        }

        public async Task<bool> IncrementCartItemAsync(string? userId, string? guestId, Guid bookId)
        {
            var cartItem = await GetCartItemAsync(userId, guestId, bookId);
            if (cartItem == null)
            {
                return false;
            }

            cartItem.Quantity++;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task MergeGuestCartAsync(string guestId, string userId)
        {
            var guestCart = await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .FirstOrDefaultAsync(x => x.GuestId == guestId);
            if(guestCart == null || !guestCart.Items.Any())
            {
                return;
            }

            var userCart = await _dbContext.ShoppingCarts
                .Include(q => q.Items)
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if(userCart == null)
            {
                guestCart.AppUserId = userId;
                guestCart.GuestId = null;
            }
            else
            {
                foreach(var guestItem in guestCart.Items.ToList())
                {
                    var existingUserItem = userCart.Items.FirstOrDefault(x => x.BookId == guestItem.BookId);
                    if(existingUserItem != null)
                    {
                        existingUserItem.Quantity += guestItem.Quantity;
                    }
                    else
                    {
                        guestItem.ShoppingCartId = userCart.Id;
                        userCart.Items.Add(guestItem);
                    }
                }

                _dbContext.ShoppingCarts.Remove(guestCart);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> RemoveCartItemAsync(string? userId, string? guestId, Guid bookId)
        {
            var cartItem = await GetCartItemAsync(userId, guestId, bookId);
            if (cartItem == null)
            {
                return false;
            }

            _dbContext.ShoppingCartItems.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task<ShoppingCartItem?> GetCartItemAsync(string? userId, string? guestId, Guid bookId)
        {
            return await _dbContext.ShoppingCartItems
                .Include(q => q.ShoppingCart)
                .FirstOrDefaultAsync(x =>
                    x.BookId == bookId && (
                    (userId != null && x.ShoppingCart.AppUserId == userId) ||
                    (guestId != null && x.ShoppingCart.GuestId == guestId)
                ));
        }
    }
}

using BookShop.Application.Interfaces;
using BookShop.Application.ViewModels.ShoppingCart;
using BookShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartRepository _cartRepository;

        public ShoppingCartController(IShoppingCartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ShoppingCart? cart;

            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                cart = await _cartRepository.GetCartForUserIdAsync(userId);
            }
            else
            {
                var guestId = GetOrCreateGuestId();
                cart = await _cartRepository.GetCartForGuestIdAsync(guestId);
            }

            if(cart == null || cart.Items == null || !cart.Items.Any())
            {
                var emptyCart = new ShoppingCartResponseVM
                {
                    Id = Guid.Empty,
                    Items = new List<ShoppingCartItemResponseVM>()
                };
                return View(emptyCart);
            }

            var shoppingCartVM = new ShoppingCartResponseVM
            {
                Id = cart.Id,
                Items = cart.Items.Select(item => new ShoppingCartItemResponseVM
                {
                    Id = item.Id,
                    BookId = item.Book.Id,
                    BookTitle = item.Book.Title,
                    BookImageUrl = item.Book.ImageUrl,
                    Price = item.Book.Price,
                    SpecialPrice = item.Book.SpecialPrice ?? 0,
                    Quantity = item.Quantity
                }).ToList()
            };

            return View(shoppingCartVM);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid bookId , int quantity)
        {
            if(quantity <= 0)
            {
                TempData["error"] = "Quantity must be at least one.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await _cartRepository.AddUserCartItemAsync(userId, bookId, quantity);
            }
            else
            {
                var guestId = GetOrCreateGuestId();
                await _cartRepository.AddGuestCartItemAsync(guestId, bookId, quantity);
            }

            TempData["success"] = "Book added to cart successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> IncrementQuantity(Guid bookId)
        {
            var userId = GetUserId();
            var guestId = GetOrCreateGuestId();

            var result = await _cartRepository.IncrementCartItemAsync(userId, guestId, bookId);
            if (!result)
            {
                TempData["error"] = "Fail to Update cart.";
            }
            else
            {
                TempData["success"] = "Cart updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> DecrementQuantity(Guid bookId)
        {
            var userId = GetUserId();
            var guestId = GetOrCreateGuestId();

            var result = await _cartRepository.DecrementCartItemAsync(userId, guestId, bookId);
            if (!result)
            {
                TempData["error"] = "Fail to Update cart.";
            }
            else
            {
                TempData["success"] = "Cart updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(Guid bookId)
        {
            var userId = GetUserId();
            var guestId = GetOrCreateGuestId();

            var result = await _cartRepository.RemoveCartItemAsync(userId, guestId, bookId);
            if (!result)
            {
                TempData["error"] = "Fail to remove cart item.";
            }
            else
            {
                TempData["success"] = "Cart item removed successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private string GetOrCreateGuestId()
        {
            var cookieName = "GuestId";
            if(Request.Cookies.TryGetValue(cookieName , out var guestId)
               && !string.IsNullOrEmpty(guestId))
            {
                return guestId;
            }

            guestId = Guid.NewGuid().ToString();
            Response.Cookies.Append(
                cookieName,
                guestId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(10)
                }
            );

            return guestId;
        }
    }
}

using BookShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.Web.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly IShoppingCartRepository _cartRepository;

        public CartCountViewComponent(IShoppingCartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int cartCount = 0;
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userClaims = User as ClaimsPrincipal;
                var userId = userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userCart = await _cartRepository.GetCartForUserIdAsync(userId);
                    cartCount = userCart?.Items
                        .Sum(x => x.Quantity) ?? 0;
                }
            }
            else
            {
                var guestId = Request.Cookies["GuestId"];
                if (!string.IsNullOrEmpty(guestId))
                {
                    var guestCart = await _cartRepository.GetCartForGuestIdAsync(guestId);
                    cartCount = guestCart?.Items
                         .Sum(x => x.Quantity) ?? 0;
                }
            }

            return View(cartCount);
        }
    }
}

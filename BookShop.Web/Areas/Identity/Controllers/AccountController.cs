using BookShop.Application.Interfaces;
using BookShop.Application.IServices;
using BookShop.Application.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IShoppingCartRepository _cartRepository;

        public AccountController(IAuthService authService ,
                                 IShoppingCartRepository cartRepository)
        {
            _authService = authService;
            _cartRepository = cartRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequestVM requestVM)
        {
            if (!ModelState.IsValid)
            {
                return View(requestVM);
            }

            var result = await _authService.Register(requestVM);
            if (!result.Success)
            {
                TempData["error"] = result.Message ?? "Registration failed";
                return View(requestVM);
            }

            TempData["success"] = result.Message ?? "Registration completed successfully.";
            return RedirectToAction(nameof(Login));
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var vm = new LoginRequestVM
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestVM requestVM)
        {
            if (!ModelState.IsValid)
            {
                return View(requestVM);
            }

            var result = await _authService.Login(requestVM);
            if (!result.Success)
            {
                TempData["error"] = result.Message ?? "Login Failed";
                return View(requestVM);
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var guestId = Request.Cookies["GuestId"];

            if(!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(guestId))
            {
                await _cartRepository.MergeGuestCartAsync(guestId, userId);
                Response.Cookies.Delete(
                    "GuestId",
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    }
                );
            }

            TempData["success"] = result.Message ?? "Login successful.";

            if(!string.IsNullOrEmpty(requestVM.ReturnUrl) && Url.IsLocalUrl(requestVM.ReturnUrl))
            {
                return Redirect(requestVM.ReturnUrl);
            }

            if(result?.Roles.Contains("ADMIN") == true)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            TempData["success"] = "Logout successfully.";
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

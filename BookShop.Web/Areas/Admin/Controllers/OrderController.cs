using BookShop.Application.Interfaces;
using BookShop.Application.ViewModels.Order;
using BookShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus? status = null , string? search = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var orders = await _orderRepository.GetAllOrdersAsync(status , search);
            var orderVMs = orders.Select(order => new OrderResponseVM
            {
                Id = order.Id,
                AppUserId = userId,

                Total = order.Total,
                TotalItems = order.TotalItems,
                DeliveryFee = order.DeliveryFee,
                GrandTotal = order.GrandTotal,

                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,

                OrderDate = order.OrderDate,
                PaymentDate = order.PaymentDate,

                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email
            }).ToList();

            return View(orderVMs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if(order == null)
            {
                return NotFound();
            }

            var orderVM = new OrderResponseVM
            {
                Id = order.Id,
                AppUserId = order.AppUserId,

                Total = order.Total,
                TotalItems = order.TotalItems,
                DeliveryFee = order.DeliveryFee,
                GrandTotal = order.GrandTotal,

                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,

                OrderDate = order.OrderDate,
                PaymentDate = order.PaymentDate,

                Items = order.Items.Select(item => new CheckoutVM
                {
                    Id = item.Id,
                    BookId = item.BookId,
                    BookTitle = item.BookTitle,
                    BookImageUrl = item.BookImageUrl,
                    Price = item.BookPrice,
                    Quantity = item.Quantity
                }).ToList(),

                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,

                Street = order.Street,
                City = order.City,
                Country = order.Country,

                Phone1 = order.Phone1,
                Phone2 = order.Phone2,
                DeliveryInstruction = order.DeliveryInstruction
            };

            return View(orderVM);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid orderId , OrderStatus status)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if(order == null)
            {
                return NotFound();
            }

            await _orderRepository.UpdateOrderStatusAsync(order.Id, status);
            TempData["success"] = "Order status updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using BookShop.Application.Interfaces;
using BookShop.Application.IServices;
using BookShop.Application.ViewModels.Dashboard;
using BookShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IAuthorRepository _authorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IOrderRepository _orderRepository;

        public DashboardController(IAuthService authService ,
                                   IAuthorRepository authorRepository ,
                                   ICategoryRepository categoryRepository ,
                                   IBookRepository bookRepository ,
                                   IOrderRepository orderRepository)
        {
            _authService = authService;
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
            _orderRepository = orderRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var today = DateTime.Now;
            var monthlyRevenue = new List<Decimal>();
            var monthlyOrders = new List<int>();

            for(int i = 5; i >= 0; i--)
            {
                var target = today.AddMonths(-i);
                var monthlyData = orders.Where(o => o.OrderDate.Month == target.Month &&
                                               o.OrderDate.Year == target.Year);
                monthlyRevenue.Add(monthlyData.Sum(o => o.GrandTotal));
                monthlyOrders.Add(monthlyData.Count());
            }

            var totalAuthors = await _authorRepository.CountAsync();
            var totalCategories = await _categoryRepository.CountAsync();
            var totalBooks = await _bookRepository.CountAsync();
            var totalOrders = await _orderRepository.CountAsync();
            var totalUsers = await _authService.CountAsync();

            var pendingOrders = orders.Count(o => o.OrderStatus == OrderStatus.Pending);
            var confirmOrders = orders.Count(o => o.OrderStatus == OrderStatus.Confirmed);
            var packedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Packed);
            var shippedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Shipped);
            var deliverdOrders = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
            var cancellOrders = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled);

            var pendingPayments = orders.Count(o => o.PaymentStatus == PaymentStatus.Pending);
            var successPayments = orders.Count(o => o.PaymentStatus == PaymentStatus.Success);
            var cancellPayments = orders.Count(o => o.PaymentStatus == PaymentStatus.Failed);

            var totalRevenue = orders.Sum(o => o.GrandTotal);

            var dashboardVM = new DashboardVM
            {
                TotalAuthors = totalAuthors,
                TotalCategories = totalCategories,
                TotalBooks = totalBooks,
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,

                PendingOrders = pendingOrders,
                ConfirmOrders = confirmOrders,
                PackedOrders = packedOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliverdOrders,
                CancellOrders = cancellOrders,

                PendingPayments = pendingPayments,
                SuccessPayments = successPayments,
                FailedPayments = cancellPayments,

                TotalRevenue = totalRevenue,
                MonthlyOrders = monthlyOrders,
                MonthlyRevenue = monthlyRevenue
            };

            return View(dashboardVM);
        }
    }
}

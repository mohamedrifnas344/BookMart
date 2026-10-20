using BookShop.Application.Interfaces;
using BookShop.Application.ViewModels.Order;
using BookShop.Domain.Entities;
using BookShop.Domain.Enums;
using BookShop.Infrastructure.Services.StripePayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "USER")]
    public class OrderController : Controller
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly StripeSettings _stripeSettings;

        public OrderController(IShoppingCartRepository cartRepository ,
                               IOrderRepository orderRepository ,
                               IOptions<StripeSettings> options)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _stripeSettings = options.Value;
        }


        [HttpGet]
        public async Task<IActionResult> MyOrders(OrderStatus? status = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var orders = await _orderRepository.GetAllOrdersByUserIdAsync(userId , status);
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

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var cart = await _cartRepository.GetCartForUserIdAsync(userId);
            if(cart == null || !cart.Items.Any())
            {
                return NotFound();
            }

            var vms = cart.Items.Select(item => new CheckoutVM
            {
                Id = item.Id,
                BookId = item.Book.Id,
                BookTitle = item.Book.Title,
                BookImageUrl = item.Book.ImageUrl,
                Price = item.Book.Price,
                SpecialPrice = item.Book.SpecialPrice ?? 0,
                Quantity = item.Quantity
            }).ToList();

            var orderVM = new OrderCreateVM
            {
                Items = vms
            };

            return View(orderVM);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(OrderCreateVM createVM)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            if (!ModelState.IsValid)
            {
                var reloadCart = await _cartRepository.GetCartForUserIdAsync(userId);
                createVM.Items = reloadCart.Items.Select(item => new CheckoutVM
                {
                    Id = item.Id,
                    BookId = item.Book.Id,
                    BookTitle = item.Book.Title,
                    BookImageUrl = item.Book.ImageUrl,
                    Price = item.Book.Price,
                    SpecialPrice = item.Book.SpecialPrice ?? 0,
                    Quantity = item.Quantity
                }).ToList();
                return View(createVM);
            }

            var cart = await _cartRepository.GetCartForUserIdAsync(userId);
            if(cart == null)
            {
                return NotFound();
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,

                FirstName = createVM.FirstName,
                LastName = createVM.LastName,
                Email = createVM.Email,

                City = createVM.City,
                Street = createVM.Street,
                Country = createVM.Country,

                Phone1 = createVM.Phone1,
                Phone2 = createVM.Phone2,
                DeliveryInstruction = createVM.DeliveryInstruction
            };

            foreach(var item in cart.Items)
            {
                var price = item.Book.SpecialPrice.HasValue &&
                    item.Book.SpecialPrice.Value > 0 ?
                    item.Book.SpecialPrice.Value : item.Book.Price;

                var orderItem = new OrderItem
                {
                    BookId = item.Book.Id,
                    BookTitle = item.Book.Title,
                    BookImageUrl = item.Book.ImageUrl,
                    BookPrice = price,
                    Quantity = item.Quantity
                };

                order.Items.Add(orderItem);
            }

            var total = order.Items.Sum(x => x.BookPrice * x.Quantity);
            var deliveryFee = 4m;
            var grandTotal = total + deliveryFee;
            var totalItems = order.Items.Sum(x => x.Quantity);

            order.Total = total;
            order.DeliveryFee = deliveryFee;
            order.GrandTotal = grandTotal;
            order.TotalItems = totalItems;

            if(createVM.PaymentMethod == Domain.Enums.PaymentMethod.CashOnDelivery)
            {
                order.PaymentMethod = Domain.Enums.PaymentMethod.CashOnDelivery;
                await _orderRepository.AddOrderAsync(order);

                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }


            //Stripe Payment
            order.PaymentMethod = Domain.Enums.PaymentMethod.Stripe;
            await _orderRepository.AddOrderAsync(order);

            Stripe.StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var domain = $"{Request.Scheme}://{Request.Host}/";

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Customer/Order/OrderConfirmation?orderId={order.Id}",
                CancelUrl = domain + $"Customer/Order/Cancell",
                LineItems = order.Items.Select(item => new SessionLineItemOptions
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.BookPrice * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.BookTitle
                        }
                    }
                }).ToList(),
                Mode = "payment"
            };

            var service = new SessionService();
            Session session = service.Create(options);

            await _orderRepository.UpdateStripeSessionDdAsync(order.Id, session.Id);
            return Redirect(session.Url);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if(order == null)
            {
                return NotFound();
            }

            if(order.PaymentMethod == Domain.Enums.PaymentMethod.CashOnDelivery)
            {
                await _cartRepository.ClearCartAsync(order.AppUserId);
            }
            else
            {
                StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
                var service = new SessionService();
                Session session = service.Get(order.SessionId);

                var status = session.PaymentStatus?.ToLower() switch
                {
                    "paid" => PaymentStatus.Success,
                    "unpaid" => PaymentStatus.Failed,
                    _ => PaymentStatus.Pending
                };

                await _orderRepository.UpdatePaymentStatusAsync(order.Id, status);
                if(status == PaymentStatus.Success)
                {
                    await _orderRepository.UpdateOrderStatusAsync(order.Id, OrderStatus.Confirmed);
                    await _cartRepository.ClearCartAsync(order.AppUserId);
                }
            }

            var orderCreationResult = new OrderCreationVM
            {
                OrderId = order.Id,
                GrandTotal = order.GrandTotal,
                PaymentMethod = order.PaymentMethod
            };

            return View(orderCreationResult);
        }
    }
}

using BookShop.Domain.Entities;
using BookShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.ViewModels.Order
{
    public class OrderResponseVM
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; } = null!;

        public decimal Total { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal GrandTotal { get; set; }
        public int TotalItems { get; set; }

        public DateTime OrderDate { get; set; } 
        public DateTime? PaymentDate { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; } 
        public PaymentMethod PaymentMethod { get; set; }

        public ICollection<CheckoutVM> Items { get; set; } = new List<CheckoutVM>();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string Phone1 { get; set; } = string.Empty;
        public string Phone2 { get; set; } = string.Empty;

        public string? DeliveryInstruction { get; set; }
    }
}

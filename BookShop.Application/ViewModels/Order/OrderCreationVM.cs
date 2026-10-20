using BookShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.ViewModels.Order
{
    public class OrderCreationVM
    {
        public Guid OrderId { get; set; }
        public decimal GrandTotal { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
    }
}

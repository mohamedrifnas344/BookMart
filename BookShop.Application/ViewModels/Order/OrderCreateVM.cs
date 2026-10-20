using BookShop.Domain.Entities;
using BookShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Application.ViewModels.Order
{
    public class OrderCreateVM
    {
        [ValidateNever]
        public ICollection<CheckoutVM> Items { get; set; } = new List<CheckoutVM>();

        public decimal Total => Items.Sum(x => x.SubTotal);
        public decimal DeliveryFee => 4m;
        public decimal GrandTotal => Total + DeliveryFee;
        public int TotalItems => Items.Sum(x => x.Quantity);


        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [DataType(DataType.PhoneNumber)]
        public string Phone1 { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [DataType(DataType.PhoneNumber)]
        public string Phone2 { get; set; } = string.Empty;


        [StringLength(500)]
        public string? DeliveryInstruction { get; set; }

        public string? SessionId { get; set; }
    }
}

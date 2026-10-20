using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Domain.Entities
{
    public class ShoppingCart
    {
        public Guid Id { get; set; }
        public string? AppUserId { get; set; }
        public string? GuestId { get; set; }

        public ICollection<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
    }
}

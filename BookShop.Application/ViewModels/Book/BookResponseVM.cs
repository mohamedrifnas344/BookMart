using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.ViewModels.Book
{
    public class BookResponseVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public int Stock { get; set; }
        public int PageNumber { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public bool IsNewArrival { get; set; }
        public DateTime PublishedDate { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}

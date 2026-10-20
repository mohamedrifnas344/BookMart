using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BookShop.Domain.Entities
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public int Stock { get; set; }
        public int PageNumber { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public bool IsNewArrival { get; set; }
        public DateTime PublishedDate { get; set; }

        //Navigation Properties
        public Guid CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        public Guid AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        public Author Author { get; set; } = null!;
    }
}

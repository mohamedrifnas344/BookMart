using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Application.ViewModels.Book
{
    public class BookEditVM
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1000)]
        public decimal Price { get; set; }

        [Range(0.01, 1000)]
        public decimal? SpecialPrice { get; set; }

        [Range(0, 100)]
        public int Stock { get; set; }

        [Range(1, 1000)]
        public int PageNumber { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 10)]
        public string ISBN { get; set; } = string.Empty;

        public bool IsNewArrival { get; set; }

        [Required]
        public DateTime PublishedDate { get; set; }

        [DisplayName("Image")]
        public IFormFile? File { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Required]
        [DisplayName("Category")]
        public Guid CategoryId { get; set; }

        [Required]
        [DisplayName("Author")]
        public Guid AuthorId { get; set; }
    }
}

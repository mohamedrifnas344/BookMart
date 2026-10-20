using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Application.ViewModels.Author
{
    public class AuthorEditVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Biography { get; set; } = string.Empty;


        [DisplayName("Image")]
        public IFormFile? File { get; set; }
        public string? ExistingImageUrl { get; set; }
    }
}

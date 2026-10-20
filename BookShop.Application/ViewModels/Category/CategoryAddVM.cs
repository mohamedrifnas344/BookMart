using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookShop.Application.ViewModels.Category
{
    public class CategoryAddVM
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100 , MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}

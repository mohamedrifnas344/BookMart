using BookShop.Application.ViewModels.Book;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.ViewModels.Common
{
    public class BookDetailsPageResponseVM
    {
        public BookResponseVM Book { get; set; } = null!;
        public ICollection<BookResponseVM> SimilarBooks { get; set; } = new List<BookResponseVM>();
    }
}

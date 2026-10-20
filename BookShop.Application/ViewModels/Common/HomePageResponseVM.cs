using BookShop.Application.ViewModels.Author;
using BookShop.Application.ViewModels.Book;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.ViewModels.Common
{
    public class HomePageResponseVM
    {
        public ICollection<AuthorResponseVM> Authors { get; set; } = new List<AuthorResponseVM>();
        public ICollection<BookResponseVM> Books { get; set; } = new List<BookResponseVM>();
    }
}

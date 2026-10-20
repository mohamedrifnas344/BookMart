using BookShop.Application.Interfaces;
using BookShop.Application.ViewModels.Author;
using BookShop.Application.ViewModels.Book;
using BookShop.Application.ViewModels.Common;
using BookShop.Application.ViewModels.Error;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;

        public HomeController(IBookRepository bookRepository ,
                              IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var newArrivalbooks = await _bookRepository.GetAllNewArrivalBooksAsync();
            var bookVms = newArrivalbooks.Select(book => new BookResponseVM
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PageNumber = book.PageNumber,
                Price = book.Price,
                SpecialPrice = book.SpecialPrice,
                ImageUrl = book.ImageUrl,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsNewArrival = book.IsNewArrival,
                CategoryName = book.Category.Name,
                AuthorName = book.Author.Name,
                Stock = book.Stock
            }).ToList();

            var authors = await _authorRepository.GetAllAsync();
            var authorVMs = authors.Select(author => new AuthorResponseVM
            {
                Id = author.Id,
                Name = author.Name,
                Biography = author.Biography,
                ImageUrl = author.ImageUrl
            }).ToList();

            var homePageResponse = new HomePageResponseVM
            {
                Authors = authorVMs,
                Books = bookVms
            };

            return View(homePageResponse);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

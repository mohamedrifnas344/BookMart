using BookShop.Application.Interfaces;
using BookShop.Application.ViewModels.Author;
using BookShop.Application.ViewModels.Book;
using BookShop.Application.ViewModels.Category;
using BookShop.Application.ViewModels.Common;
using BookShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookController(IBookRepository bookRepository ,
                              IAuthorRepository authorRepository ,
                              ICategoryRepository categoryRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search = null , 
                                               string? category = null , 
                                               string? author = null)
        {
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Author = author;

            var books = await _bookRepository.GetAllAsync(search , category , author);
            var bookVMs = books.Select(book => MapToBookResponseVM(book)).ToList();

            var authors = await _authorRepository.GetAllAsync();
            var authorVMs = authors.Select(author => new AuthorResponseVM
            {
                Id = author.Id,
                Name = author.Name,
                Biography = author.Biography,
                ImageUrl = author.ImageUrl
            }).ToList();

            var categories = await _categoryRepository.GetAllAsync();
            var categoryVMs = categories.Select(category => new CategoryResponseVM
            {
                Id = category.Id,
                Name = category.Name
            }).ToList();

            var bookResponse = new BookPageResponseVM
            {
                Categories = categoryVMs,
                Authors = authorVMs,
                Books = bookVMs
            };

            return View(bookResponse);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookRepository.GetAsync(id);
            if(book == null)
            {
                return NotFound();
            }

            var bookVM = MapToBookResponseVM(book);

            var similarBooks = await _bookRepository.GetAllSimilarBooksAsync(book.Id, book.Category.Id);
            var similarBookVMs = similarBooks.Select(book => MapToBookResponseVM(book)).ToList();

            var bookDetailsResponse = new BookDetailsPageResponseVM
            {
                Book = bookVM,
                SimilarBooks = similarBookVMs
            };

            return View(bookDetailsResponse);
        }


        private BookResponseVM MapToBookResponseVM(Book book)
        {
            return new BookResponseVM
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
            };
        }
    }
}

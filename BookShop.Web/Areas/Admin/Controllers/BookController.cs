using BookShop.Application.Interfaces;
using BookShop.Application.IServices;
using BookShop.Application.ViewModels.Book;
using BookShop.Domain.Entities;
using BookShop.Infrastructure.Services.CloudinaryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ICloudinaryImage _cloudinaryImage;

        public BookController(IBookRepository bookRepository ,
                              ICategoryRepository categoryRepository ,
                              IAuthorRepository authorRepository ,
                              ICloudinaryImage cloudinaryImage)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _authorRepository = authorRepository;
            _cloudinaryImage = cloudinaryImage;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepository.GetAllAsync();
            var bookVMs = books.Select(book => new BookResponseVM
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
            return View(bookVMs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookRepository.GetAsync(id);
            if(book == null)
            {
                return NotFound();
            }

            var vm = new BookResponseVM
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

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await LoadDropdownsAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(BookAddVM addVM)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(addVM);
            }

            //Upload Image to Cloudinary
            string imageUrl;
            string publicId;
            try
            {
                var uploadResult = await _cloudinaryImage.UploadImageAsync(addVM.File);
                imageUrl = uploadResult.SecureUrl.ToString();
                publicId = uploadResult.PublicId;
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Failed to upload image");
                await LoadDropdownsAsync();
                return View(addVM);
            }

            var book = new Book
            {
                Title = addVM.Title,
                Description = addVM.Description,
                Price = addVM.Price,
                SpecialPrice = addVM.SpecialPrice,
                Stock = addVM.Stock,
                PageNumber = addVM.PageNumber,
                ISBN = addVM.ISBN,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsNewArrival = addVM.IsNewArrival,
                CategoryId = addVM.CategoryId,
                AuthorId = addVM.AuthorId,
                PublishedDate = addVM.PublishedDate
            };

            await _bookRepository.AddAsync(book);
            TempData["success"] = "Book created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookRepository.GetAsync(id);
            if(book == null)
            {
                return NotFound();
            }

            await LoadDropdownsAsync();

            var vm = new BookEditVM
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PageNumber = book.PageNumber,
                Price = book.Price,
                SpecialPrice = book.SpecialPrice,
                ExistingImageUrl = book.ImageUrl,
                PublishedDate = book.PublishedDate,
                ISBN = book.ISBN,
                IsNewArrival = book.IsNewArrival,
                CategoryId = book.Category.Id,
                AuthorId = book.Author.Id,
                Stock = book.Stock
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookEditVM editVM)
        {

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(editVM);
            }

            var book = await _bookRepository.GetAsync(editVM.Id);
            if(book == null)
            {
                return NotFound();
            }

            book.Title = editVM.Title;
            book.Description = editVM.Description;
            book.Price = editVM.Price;
            book.SpecialPrice = editVM.SpecialPrice;
            book.Stock = editVM.Stock;
            book.PageNumber = editVM.PageNumber;
            book.ISBN = editVM.ISBN;
            book.IsNewArrival = editVM.IsNewArrival;
            book.AuthorId = editVM.AuthorId;
            book.CategoryId = editVM.CategoryId;
            book.PublishedDate = editVM.PublishedDate;

            if (editVM.File != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(book.PublicId))
                    {
                        await _cloudinaryImage.DeleteImageAsync(book.PublicId);
                    }

                    var uploadResult = await _cloudinaryImage.UploadImageAsync(editVM.File);
                    book.ImageUrl = uploadResult.SecureUrl.ToString();
                    book.PublicId = uploadResult.PublicId;
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Failed to upload image");
                    await LoadDropdownsAsync();
                    return View(editVM);
                }
            }

            await _bookRepository.UpdateAsync();
            TempData["success"] = "Book updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _bookRepository.GetAsync(id);
            if(book == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(book.PublicId))
            {
                await _cloudinaryImage.DeleteImageAsync(book.PublicId);
            }

            await _bookRepository.DeleteAsync(id);
            TempData["success"] = "Book deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        private async Task LoadDropdownsAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var authors = await _authorRepository.GetAllAsync();

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            ViewBag.Authors = new SelectList(authors, "Id", "Name");
        }
    }
}

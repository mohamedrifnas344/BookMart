using BookShop.Application.Interfaces;
using BookShop.Application.IServices;
using BookShop.Application.ViewModels.Author;
using BookShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class AuthorController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ICloudinaryImage _cloudinaryImage;

        public AuthorController(IAuthorRepository authorRepository ,
                                ICloudinaryImage cloudinaryImage)
        {
            _authorRepository = authorRepository;
            _cloudinaryImage = cloudinaryImage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authors = await _authorRepository.GetAllAsync();
            var authorVMs = authors.Select(author => new AuthorResponseVM
            {
                Id = author.Id,
                Name = author.Name,
                Biography = author.Biography,
                ImageUrl = author.ImageUrl
            }).ToList();

            return View(authorVMs);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AuthorAddVM addVM)
        {
            if (!ModelState.IsValid)
            {
                return View(addVM);
            }

            //Upload Iamge
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
                return View(addVM);
            }

            var author = new Author
            {
                Name = addVM.Name,
                Biography = addVM.Biography,
                ImageUrl = imageUrl,
                PublicId = publicId
            };

            await _authorRepository.AddAsync(author);
            TempData["success"] = "Author created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var author = await _authorRepository.GetAsync(id);
            if(author == null)
            {
                return NotFound();
            }

            var vm = new AuthorEditVM
            {
                Id = author.Id,
                Name = author.Name,
                Biography = author.Biography,
                ExistingImageUrl = author.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AuthorEditVM editVM)
        {
            if (!ModelState.IsValid)
            {
                return View(editVM);
            }

            var author = await _authorRepository.GetAsync(editVM.Id);
            if(author == null)
            {
                return NotFound();
            }

            author.Name = editVM.Name;
            author.Biography = editVM.Biography;

            if(editVM.File != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(author.PublicId))
                    {
                        await _cloudinaryImage.DeleteImageAsync(author.PublicId);
                    }

                    var uploadResult = await _cloudinaryImage.UploadImageAsync(editVM.File);
                    author.ImageUrl = uploadResult.SecureUrl.ToString();
                    author.PublicId = uploadResult.PublicId;
                }
                catch (Exception)
                {
                    ModelState.AddModelError(string.Empty, "Failed to upload image");
                    return View(editVM);
                }
            }

            await _authorRepository.UpdateAsync();
            TempData["success"] = "Auhtor updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var author = await _authorRepository.GetAsync(id);
            if(author == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(author.PublicId))
            {
                await _cloudinaryImage.DeleteImageAsync(author.PublicId);
            }

            await _authorRepository.DeleteAsync(id);
            TempData["success"] = "Author deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

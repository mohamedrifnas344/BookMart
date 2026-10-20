using BookShop.Application.Interfaces;
using BookShop.Domain.Entities;
using BookShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public BookRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Book book)
        {
            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _dbContext.Books.CountAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _dbContext.Books.FindAsync(id);
            if(book == null)
            {
                return;
            }

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Book>> GetAllAsync(string? search = null,
                                                         string? category = null,
                                                         string? author = null)
        {
            var query = _dbContext.Books
                        .Include(q => q.Category)
                        .Include(q => q.Author)
                        .AsNoTracking()
                        .AsQueryable();

            //Seaching
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(b =>
                                    b.Title.Contains(search) ||
                                    b.Category.Name.Contains(search) ||
                                    b.Author.Name.Contains(search));
            }

            //Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category.Name.ToLower() == category.ToLower());
            }

            //Filter by author
            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Name.ToLower() == author.ToLower());
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAllNewArrivalBooksAsync()
        {
            return await _dbContext.Books
                .Include(q => q.Category)
                .Include(q => q.Author)
                .Where(b => b.IsNewArrival)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAllSimilarBooksAsync(Guid bookId, Guid categoryId)
        {
            return await _dbContext.Books
                .Include(c => c.Category)
                .Include(a => a.Author)
                .Where(b => b.CategoryId == categoryId
                            && b.Id != bookId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Book?> GetAsync(Guid id)
        {
            return await _dbContext.Books
                .Include(q => q.Category)
                .Include(q => q.Author)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

using BookShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(string? search = null,
                                            string? category = null,
                                            string? author = null);
        Task<Book?> GetAsync(Guid id);
        Task AddAsync(Book book);
        Task UpdateAsync();
        Task DeleteAsync(Guid id);
        Task<int> CountAsync();
        Task<IEnumerable<Book>> GetAllNewArrivalBooksAsync();
        Task<IEnumerable<Book>> GetAllSimilarBooksAsync(Guid bookId, Guid categoryId);
    }
}

using BookShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task AddAsync(Category category);
        Task<Category?> GetAsync(Guid id);
        Task UpdateAsync();
        Task DeleteAsync(Guid id);
        Task<int> CountAsync();
    }
}

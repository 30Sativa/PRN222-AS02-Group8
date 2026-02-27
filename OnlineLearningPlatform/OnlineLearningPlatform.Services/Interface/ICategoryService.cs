using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Category;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int categoryId);
        Task<CategoryCommandResult> CreateAsync(string categoryName);
        Task<CategoryCommandResult> UpdateAsync(int categoryId, string categoryName);
        Task<CategoryCommandResult> DeleteAsync(int categoryId);
    }
}

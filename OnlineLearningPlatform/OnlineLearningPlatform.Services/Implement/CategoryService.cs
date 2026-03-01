using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Category;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            return await _categoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<CategoryCommandResult> CreateAsync(string categoryName)
        {
            var normalizedName = NormalizeName(categoryName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return Fail("Category name is required.");
            }

            if (normalizedName.Length > 100)
            {
                return Fail("Category name must not exceed 100 characters.");
            }

            var existingCategory = await _categoryRepository.GetByNameAsync(normalizedName);
            if (existingCategory != null)
            {
                return Fail("Category name already exists.");
            }

            var category = new Category
            {
                CategoryName = normalizedName
            };

            var created = await _categoryRepository.CreateAsync(category);
            return Success("Category created successfully.", created);
        }

        public async Task<CategoryCommandResult> UpdateAsync(int categoryId, string categoryName)
        {
            var normalizedName = NormalizeName(categoryName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return Fail("Category name is required.");
            }

            if (normalizedName.Length > 100)
            {
                return Fail("Category name must not exceed 100 characters.");
            }

            var existing = await _categoryRepository.GetByIdAsync(categoryId);
            if (existing == null)
            {
                return Fail("Category not found.");
            }

            var duplicated = await _categoryRepository.GetByNameAsync(normalizedName);
            if (duplicated != null && duplicated.CategoryId != categoryId)
            {
                return Fail("Category name already exists.");
            }

            existing.CategoryName = normalizedName;
            var updated = await _categoryRepository.UpdateAsync(existing);

            if (!updated)
            {
                return Fail("Unable to update category.");
            }

            return Success("Category updated successfully.", existing);
        }

        public async Task<CategoryCommandResult> DeleteAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return Fail("Category not found.");
            }

            var hasCourses = await _categoryRepository.HasCoursesAsync(categoryId);
            if (hasCourses)
            {
                return Fail("Cannot delete category because it still has active courses.");
            }

            var deleted = await _categoryRepository.DeleteAsync(categoryId);
            if (!deleted)
            {
                return Fail("Unable to delete category.");
            }

            return Success("Category deleted successfully.", category);
        }

        private static string NormalizeName(string categoryName)
        {
            return categoryName?.Trim() ?? string.Empty;
        }

        private static CategoryCommandResult Fail(string message)
        {
            return new CategoryCommandResult
            {
                Success = false,
                Message = message
            };
        }

        private static CategoryCommandResult Success(string message, Category category)
        {
            return new CategoryCommandResult
            {
                Success = true,
                Message = message,
                Category = category
            };
        }
    }
}

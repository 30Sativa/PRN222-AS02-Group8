using CategoryEntity = OnlineLearningPlatform.Models.Entities.Category;

namespace OnlineLearningPlatform.Services.DTOs.Category
{
    public class CategoryCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CategoryEntity? Category { get; set; }
    }
}

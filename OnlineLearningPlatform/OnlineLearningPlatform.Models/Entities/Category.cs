using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Danh mục khóa học (ví dụ: Frontend, Backend, Mobile...).
    /// </summary>
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = default!;

        /// <summary>
        /// Danh sách khóa học thuộc danh mục này.
        /// </summary>
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}


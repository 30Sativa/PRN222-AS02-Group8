using System;

namespace OnlineLearningPlatform.Services.DTOs.Review
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public Guid CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

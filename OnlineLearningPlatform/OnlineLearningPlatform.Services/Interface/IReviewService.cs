using OnlineLearningPlatform.Services.DTOs.Review;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IReviewService
    {
        Task<ReviewDto?> GetUserReviewAsync(string userId, Guid courseId);
        Task<List<ReviewDto>> GetCourseReviewsAsync(Guid courseId, int page = 1, int pageSize = 10);
        Task<int> GetCourseReviewCountAsync(Guid courseId);
        Task<(double average, int count)> GetCourseRatingStatsAsync(Guid courseId);
        Task<bool> SubmitReviewAsync(string userId, Guid courseId, int rating, string comment);
        Task<bool> DeleteReviewAsync(int reviewId, string userId);
    }
}

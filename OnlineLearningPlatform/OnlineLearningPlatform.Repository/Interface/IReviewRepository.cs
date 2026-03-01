using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IReviewRepository
    {
        Task<Review?> GetByUserAndCourseAsync(string userId, Guid courseId);
        Task<List<Review>> GetByCourseIdAsync(Guid courseId, int skip, int take);
        Task<int> GetCountByCourseIdAsync(Guid courseId);
        Task<double> GetAverageRatingByCourseIdAsync(Guid courseId);
        Task<Review> CreateAsync(Review review);
        Task<bool> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int reviewId);
    }
}

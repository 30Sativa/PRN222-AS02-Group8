using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Review;
using OnlineLearningPlatform.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Implement
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _reviewRepository = reviewRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<ReviewDto?> GetUserReviewAsync(string userId, Guid courseId)
        {
            var review = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
            if (review == null) return null;

            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                CourseId = review.CourseId,
                UserId = review.UserId,
                UserName = review.User?.FullName ?? review.User?.UserName ?? "Student",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        public async Task<List<ReviewDto>> GetCourseReviewsAsync(Guid courseId, int page = 1, int pageSize = 10)
        {
            int skip = (page - 1) * pageSize;
            var reviews = await _reviewRepository.GetByCourseIdAsync(courseId, skip, pageSize);
            
            return reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                CourseId = r.CourseId,
                UserId = r.UserId,
                UserName = r.User?.FullName ?? r.User?.UserName ?? "Student",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
        }

        public async Task<int> GetCourseReviewCountAsync(Guid courseId)
        {
            return await _reviewRepository.GetCountByCourseIdAsync(courseId);
        }

        public async Task<(double average, int count)> GetCourseRatingStatsAsync(Guid courseId)
        {
            var avg = await _reviewRepository.GetAverageRatingByCourseIdAsync(courseId);
            var count = await _reviewRepository.GetCountByCourseIdAsync(courseId);
            return (avg, count);
        }

        public async Task<bool> SubmitReviewAsync(string userId, Guid courseId, int rating, string comment)
        {
            // Verify enrollment (user must be enrolled to review)
            bool isEnrolled = await _enrollmentRepository.IsEnrolledAsync(userId, courseId);
            if (!isEnrolled)
            {
                return false; // Not enrolled
            }

            // Check if already reviewed
            var existingReview = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
            if (existingReview != null)
            {
                // Update
                existingReview.Rating = rating;
                existingReview.Comment = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment;
                return await _reviewRepository.UpdateAsync(existingReview);
            }

            // Create new
            var newReview = new Review
            {
                UserId = userId,
                CourseId = courseId,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.CreateAsync(newReview);
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
        {
            // Simplified deletion check for demo -> just delete by Id 
            return await _reviewRepository.DeleteAsync(reviewId);
        }
    }
}

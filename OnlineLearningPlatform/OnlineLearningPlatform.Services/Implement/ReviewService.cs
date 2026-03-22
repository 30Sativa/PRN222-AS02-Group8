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
            return MapToDto(review);
        }

        public async Task<List<ReviewDto>> GetCourseReviewsAsync(Guid courseId, int page = 1, int pageSize = 10)
        {
            int skip = (page - 1) * pageSize;
            var reviews = await _reviewRepository.GetByCourseIdAsync(courseId, skip, pageSize);
            return reviews.Select(MapToDto).ToList();
        }

        public async Task<int> GetCourseReviewCountAsync(Guid courseId)
        {
            return await _reviewRepository.GetCountByCourseIdAsync(courseId);
        }

        public async Task<(double average, int count)> GetCourseRatingStatsAsync(Guid courseId)
        {
            var avg   = await _reviewRepository.GetAverageRatingByCourseIdAsync(courseId);
            var count = await _reviewRepository.GetCountByCourseIdAsync(courseId);
            return (avg, count);
        }

        public async Task<RatingBreakdownDto> GetRatingBreakdownAsync(Guid courseId)
        {
            // Gọi xuống Repository — đúng pattern Service → Repository → DbContext
            var breakdown = await _reviewRepository.GetRatingBreakdownAsync(courseId);

            var dto = new RatingBreakdownDto
            {
                Star5 = breakdown.GetValueOrDefault(5),
                Star4 = breakdown.GetValueOrDefault(4),
                Star3 = breakdown.GetValueOrDefault(3),
                Star2 = breakdown.GetValueOrDefault(2),
                Star1 = breakdown.GetValueOrDefault(1),
            };
            dto.Total = dto.Star5 + dto.Star4 + dto.Star3 + dto.Star2 + dto.Star1;
            dto.Average = dto.Total == 0 ? 0
                : (double)(dto.Star5 * 5 + dto.Star4 * 4 + dto.Star3 * 3 + dto.Star2 * 2 + dto.Star1 * 1) / dto.Total;
            return dto;
        }

        public async Task<bool> SubmitReviewAsync(string userId, Guid courseId, int rating, string comment)
        {
            bool isEnrolled = await _enrollmentRepository.IsEnrolledAsync(userId, courseId);
            if (!isEnrolled) return false;

            var existingReview = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
            if (existingReview != null)
            {
                existingReview.Rating  = rating;
                existingReview.Comment = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment;
                return await _reviewRepository.UpdateAsync(existingReview);
            }

            var newReview = new Review
            {
                UserId    = userId,
                CourseId  = courseId,
                Rating    = rating,
                Comment   = string.IsNullOrWhiteSpace(comment) ? string.Empty : comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.CreateAsync(newReview);
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
        {
            return await _reviewRepository.DeleteAsync(reviewId);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static ReviewDto MapToDto(Review r) => new ReviewDto
        {
            ReviewId      = r.ReviewId,
            CourseId      = r.CourseId,
            UserId        = r.UserId,
            UserName      = r.User?.FullName ?? r.User?.UserName ?? "Học viên",
            UserAvatarUrl = null,   // ApplicationUser chưa có cột AvatarUrl
            Rating        = r.Rating,
            Comment       = r.Comment,
            CreatedAt     = r.CreatedAt,
            UpdatedAt     = r.UpdatedAt
        };
    }
}

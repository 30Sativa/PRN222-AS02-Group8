using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByUserAndCourseAsync(string userId, Guid courseId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId && !r.IsDeleted);
        }

        public async Task<List<Review>> GetByCourseIdAsync(Guid courseId, int skip, int take)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetCountByCourseIdAsync(Guid courseId)
        {
            return await _context.Reviews
                .CountAsync(r => r.CourseId == courseId && !r.IsDeleted);
        }

        public async Task<double> GetAverageRatingByCourseIdAsync(Guid courseId)
        {
            var count = await GetCountByCourseIdAsync(courseId);
            if (count == 0) return 0;
            return await _context.Reviews
                .Where(r => r.CourseId == courseId && !r.IsDeleted)
                .AverageAsync(r => r.Rating);
        }

        public async Task<Review> CreateAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> UpdateAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null) return false;
            
            review.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

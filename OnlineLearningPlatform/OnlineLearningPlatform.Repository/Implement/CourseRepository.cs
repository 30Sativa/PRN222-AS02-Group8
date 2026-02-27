using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetByTeacherAsync(string teacherId)
        {
            return await _context.Courses
                .Where(c => c.TeacherId == teacherId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdAndTeacherAsync(Guid courseId, string teacherId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId && !c.IsDeleted);
        }

        public async Task<List<Course>> GetAllForAdminAsync()
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(Guid courseId)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string courseCode, Guid? excludeCourseId = null)
        {
            var normalizedCode = courseCode.Trim().ToLower();
            return await _context.Courses
                .AnyAsync(c => c.CourseCode.ToLower() == normalizedCode
                               && (!excludeCourseId.HasValue || c.CourseId != excludeCourseId.Value));
        }

        public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeCourseId = null)
        {
            var normalizedSlug = slug.Trim().ToLower();
            return await _context.Courses
                .AnyAsync(c => c.Slug.ToLower() == normalizedSlug
                               && (!excludeCourseId.HasValue || c.CourseId != excludeCourseId.Value));
        }

        public async Task<Course> CreateAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> UpdateAsync(Course course)
        {
            var existing = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == course.CourseId && c.TeacherId == course.TeacherId && !c.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            existing.Title = course.Title;
            existing.CourseCode = course.CourseCode;
            existing.Slug = course.Slug;
            existing.Description = course.Description;
            existing.CategoryId = course.CategoryId;
            existing.Price = course.Price;
            existing.DiscountPrice = course.DiscountPrice;
            existing.Level = course.Level;
            existing.Language = course.Language;
            existing.ThumbnailUrl = course.ThumbnailUrl;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid courseId, string teacherId)
        {
            var existing = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacherId && !c.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            if (existing.Status == CourseStatus.Pending)
            {
                _context.Courses.Remove(existing);
            }
            else
            {
                existing.IsDeleted = true;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAsync(Guid courseId)
        {
            var existing = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            existing.Status = CourseStatus.Published;
            existing.RejectionReason = null;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(Guid courseId, string reason)
        {
            var existing = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && !c.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            existing.Status = CourseStatus.Rejected;
            existing.RejectionReason = reason.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

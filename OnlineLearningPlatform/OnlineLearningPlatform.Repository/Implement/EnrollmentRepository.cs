using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> IsEnrolledAsync(string userId, Guid courseId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && e.IsActive);
        }

        public async Task<Enrollment?> GetByUserAndCourseAsync(string userId, Guid courseId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections.OrderBy(s => s.OrderIndex))
                        .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.IsActive);
        }

        public async Task<List<Enrollment>> GetByUserIdAsync(string userId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections.OrderBy(s => s.OrderIndex))
                        .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Where(e => e.UserId == userId && e.IsActive)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetByIdAsync(Guid enrollmentId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
        }

        public async Task<bool> UpdateAsync(Enrollment enrollment)
        {
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollment.EnrollmentId);

            if (existing == null) return false;

            existing.IsActive = enrollment.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountByCourseAsync(Guid courseId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.CourseId == courseId && e.IsActive);
        }

        public async Task<List<Enrollment>> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.User)
                .Where(e => e.CourseId == courseId && e.IsActive)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task UpdateLastAccessedAsync(string userId, Guid courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && e.IsActive);

            if (enrollment != null)
            {
                enrollment.LastAccessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}

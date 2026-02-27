using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Implement
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        public StudentService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Course>> GetAllCoursesWithEnrollmentStatusAsync(string userId)
        {
            return await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                        .ThenInclude(l => l.Quizzes)
                .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
                .ToListAsync();
        }

        public async Task EnrollInCourseAsync(string userId, Guid courseId)
        {
            var alreadyEnrolled = await IsEnrolledAsync(userId, courseId);
            if (alreadyEnrolled) return;

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                CourseId = courseId,
                UserId = userId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEnrolledAsync(string userId, Guid courseId)
        {
            return await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        }
    }
}

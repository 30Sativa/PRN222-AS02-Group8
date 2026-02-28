using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        public CourseRepository(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Course>> GetCoursesWithSectionsAndQuizzesAsync() =>
            await _context.Courses.Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Quizzes)
                .Where(c => !c.IsDeleted).ToListAsync();

        public async Task<IEnumerable<Course>> GetPublishedCoursesWithEnrollmentsAsync() =>
            await _context.Courses.Include(c => c.Enrollments).Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Quizzes)
                .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published).ToListAsync();
    }
}

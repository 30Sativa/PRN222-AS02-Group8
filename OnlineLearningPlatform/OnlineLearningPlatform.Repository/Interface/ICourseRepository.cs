using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetByTeacherAsync(string teacherId);
        Task<Course?> GetByIdAndTeacherAsync(Guid courseId, string teacherId);

        Task<List<Course>> GetAllForAdminAsync();
        Task<Course?> GetByIdAsync(Guid courseId);

        Task<bool> ExistsByCodeAsync(string courseCode, Guid? excludeCourseId = null);
        Task<bool> ExistsBySlugAsync(string slug, Guid? excludeCourseId = null);

        Task<Course> CreateAsync(Course course);
        Task<bool> UpdateAsync(Course course);
        Task<bool> DeleteAsync(Guid courseId, string teacherId);

        Task<bool> ApproveAsync(Guid courseId);
        Task<bool> RejectAsync(Guid courseId, string reason);
        Task<IEnumerable<Course>> GetCoursesWithSectionsAndQuizzesAsync();
        Task<IEnumerable<Course>> GetPublishedCoursesWithEnrollmentsAsync();
    }
}

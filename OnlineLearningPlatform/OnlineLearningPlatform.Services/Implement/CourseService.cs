using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Course;
using OnlineLearningPlatform.Services.DTOs.Course.Request;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<List<Course>> GetMyCoursesAsync(string teacherId)
        {
            return await _courseRepository.GetByTeacherAsync(teacherId);
        }

        public async Task<Course?> GetMyCourseByIdAsync(Guid courseId, string teacherId)
        {
            return await _courseRepository.GetByIdAndTeacherAsync(courseId, teacherId);
        }

        public async Task<CourseCommandResult> CreateAsync(string teacherId, CourseUpsertRequest request)
        {
            var validation = await ValidateRequestAsync(request, null);
            if (!validation.Success)
            {
                return validation;
            }

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                TeacherId = teacherId,
                CourseCode = request.CourseCode.Trim(),
                Title = request.Title.Trim(),
                Slug = request.Slug.Trim(),
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Level = request.Level,
                Language = string.IsNullOrWhiteSpace(request.Language) ? "vi" : request.Language.Trim(),
                ThumbnailUrl = request.ThumbnailUrl?.Trim(),
                Status = CourseStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _courseRepository.CreateAsync(course);
            return Success("Course created successfully.", created);
        }

        public async Task<CourseCommandResult> UpdateAsync(string teacherId, CourseUpsertRequest request)
        {
            if (!request.CourseId.HasValue)
            {
                return Fail("CourseId is required for update.");
            }

            var existing = await _courseRepository.GetByIdAndTeacherAsync(request.CourseId.Value, teacherId);
            if (existing == null)
            {
                return Fail("Course not found or you do not have permission.");
            }

            var validation = await ValidateRequestAsync(request, request.CourseId.Value);
            if (!validation.Success)
            {
                return validation;
            }

            existing.CourseCode = request.CourseCode.Trim();
            existing.Title = request.Title.Trim();
            existing.Slug = request.Slug.Trim();
            existing.Description = request.Description?.Trim();
            existing.CategoryId = request.CategoryId;
            existing.Price = request.Price;
            existing.DiscountPrice = request.DiscountPrice;
            existing.Level = request.Level;
            existing.Language = string.IsNullOrWhiteSpace(request.Language) ? "vi" : request.Language.Trim();
            existing.ThumbnailUrl = request.ThumbnailUrl?.Trim();

            var updated = await _courseRepository.UpdateAsync(existing);
            if (!updated)
            {
                return Fail("Unable to update course.");
            }

            return Success("Course updated successfully.", existing);
        }

        public async Task<CourseCommandResult> DeleteAsync(Guid courseId, string teacherId)
        {
            var existing = await _courseRepository.GetByIdAndTeacherAsync(courseId, teacherId);
            if (existing == null)
            {
                return Fail("Course not found or you do not have permission.");
            }

            var deleted = await _courseRepository.DeleteAsync(courseId, teacherId);
            if (!deleted)
            {
                return Fail("Unable to delete course.");
            }

            return Success("Course deleted successfully.", existing);
        }

        private async Task<CourseCommandResult> ValidateRequestAsync(CourseUpsertRequest request, Guid? excludeCourseId)
        {
            if (string.IsNullOrWhiteSpace(request.CourseCode))
            {
                return Fail("Course code is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Fail("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                return Fail("Slug is required.");
            }

            if (request.DiscountPrice.HasValue && request.DiscountPrice.Value > request.Price)
            {
                return Fail("Discount price cannot be greater than price.");
            }

            if (await _courseRepository.ExistsByCodeAsync(request.CourseCode, excludeCourseId))
            {
                return Fail("Course code already exists.");
            }

            if (await _courseRepository.ExistsBySlugAsync(request.Slug, excludeCourseId))
            {
                return Fail("Course slug already exists.");
            }

            return new CourseCommandResult { Success = true };
        }

        private static CourseCommandResult Fail(string message)
        {
            return new CourseCommandResult
            {
                Success = false,
                Message = message
            };
        }

        private static CourseCommandResult Success(string message, Course course)
        {
            return new CourseCommandResult
            {
                Success = true,
                Message = message,
                Course = course
            };
        }
    }
}

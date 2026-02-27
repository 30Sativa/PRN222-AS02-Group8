using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class LessonRepository : ILessonRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lesson>> GetBySectionAsync(int sectionId)
        {
            return await _context.Lessons
                .Where(l => l.SectionId == sectionId && !l.IsDeleted)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByIdAsync(int lessonId)
        {
            return await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && !l.IsDeleted);
        }

        public async Task<Lesson> CreateAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return lesson;
        }

        public async Task<bool> UpdateAsync(Lesson lesson)
        {
            var existing = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == lesson.LessonId && !l.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            existing.Title = lesson.Title;
            existing.LessonType = lesson.LessonType;
            existing.OrderIndex = lesson.OrderIndex;
            existing.IsPreview = lesson.IsPreview;
            existing.VideoStoragePath = lesson.VideoStoragePath;
            existing.VideoOriginalFileName = lesson.VideoOriginalFileName;
            existing.VideoDurationSeconds = lesson.VideoDurationSeconds;
            existing.VideoStatus = lesson.VideoStatus;
            existing.Content = lesson.Content;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int lessonId)
        {
            var existing = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && !l.IsDeleted);

            if (existing == null)
            {
                return false;
            }

            existing.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsLessonInTeacherCourseAsync(int lessonId, string teacherId)
        {
            return await _context.Lessons
                .AnyAsync(l => l.LessonId == lessonId
                               && !l.IsDeleted
                               && l.Section.Course.TeacherId == teacherId
                               && !l.Section.Course.IsDeleted);
        }

        public async Task<bool> ReorderAsync(int sectionId, List<int> orderedLessonIds)
        {
            var lessons = await _context.Lessons
                .Where(l => l.SectionId == sectionId && !l.IsDeleted)
                .ToListAsync();

            if (!lessons.Any())
            {
                return false;
            }

            var allMatch = lessons.Select(l => l.LessonId).OrderBy(x => x)
                .SequenceEqual(orderedLessonIds.OrderBy(x => x));

            if (!allMatch)
            {
                return false;
            }

            for (int i = 0; i < orderedLessonIds.Count; i++)
            {
                var lesson = lessons.First(l => l.LessonId == orderedLessonIds[i]);
                lesson.OrderIndex = i + 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

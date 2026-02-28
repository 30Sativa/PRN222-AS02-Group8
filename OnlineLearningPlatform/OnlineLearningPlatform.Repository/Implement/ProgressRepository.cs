using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public ProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LessonProgress?> GetAsync(string userId, int lessonId)
        {
            return await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);
        }

        public async Task<List<LessonProgress>> GetByCourseAsync(string userId, Guid courseId)
        {
            return await _context.LessonProgresses
                .Include(p => p.Lesson)
                    .ThenInclude(l => l.Section)
                .Where(p => p.UserId == userId
                         && p.Lesson.Section.CourseId == courseId
                         && !p.Lesson.IsDeleted)
                .ToListAsync();
        }

        public async Task<LessonProgress> UpsertAsync(LessonProgress progress)
        {
            var existing = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.LessonId == progress.LessonId);

            if (existing == null)
            {
                progress.ProgressId = Guid.NewGuid();
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                existing.IsCompleted = progress.IsCompleted;
                existing.WatchedSeconds = progress.WatchedSeconds;
                existing.CompletedAt = progress.CompletedAt;
            }

            await _context.SaveChangesAsync();
            return existing ?? progress;
        }

        public async Task<bool> MarkCompleteAsync(string userId, int lessonId)
        {
            var existing = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (existing == null)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    ProgressId = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.IsCompleted = true;
                existing.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateWatchedSecondsAsync(string userId, int lessonId, int watchedSeconds)
        {
            var existing = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (existing == null)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    ProgressId = Guid.NewGuid(),
                    UserId = userId,
                    LessonId = lessonId,
                    WatchedSeconds = watchedSeconds
                });
            }
            else
            {
                existing.WatchedSeconds = watchedSeconds;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountCompletedAsync(string userId, Guid courseId)
        {
            return await _context.LessonProgresses
                .CountAsync(p => p.UserId == userId
                              && p.IsCompleted
                              && p.Lesson.Section.CourseId == courseId
                              && !p.Lesson.IsDeleted);
        }

        public async Task<int> CountTotalLessonsAsync(Guid courseId)
        {
            return await _context.Lessons
                .CountAsync(l => l.Section.CourseId == courseId && !l.IsDeleted);
        }

        public async Task<int?> GetNextIncompleteLessonIdAsync(string userId, Guid courseId)
        {
            // Lấy tất cả lesson id đã hoàn thành
            var completedLessonIds = await _context.LessonProgresses
                .Where(p => p.UserId == userId && p.IsCompleted)
                .Select(p => p.LessonId)
                .ToListAsync();

            // Lấy bài đầu tiên chưa hoàn thành (theo thứ tự Section.OrderIndex, Lesson.OrderIndex)
            var nextLesson = await _context.Lessons
                .Include(l => l.Section)
                .Where(l => l.Section.CourseId == courseId
                         && !l.IsDeleted
                         && !completedLessonIds.Contains(l.LessonId))
                .OrderBy(l => l.Section.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .Select(l => (int?)l.LessonId)
                .FirstOrDefaultAsync();

            return nextLesson;
        }
    }
}

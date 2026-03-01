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
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                .Include(l => l.Assignments)
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
            existing.ReadingPdfStoragePath = lesson.ReadingPdfStoragePath;
            existing.ReadingPdfOriginalFileName = lesson.ReadingPdfOriginalFileName;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int lessonId)
        {
            var existing = await _context.Lessons
                .Include(l => l.LessonProgresses)
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.QuizAttempts)
                        .ThenInclude(qa => qa.QuizAnswers)
                .Include(l => l.Assignments)
                    .ThenInclude(a => a.AssignmentSubmissions)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (existing == null)
            {
                return false;
            }

            var quizAnswers = existing.Quizzes
                .SelectMany(q => q.QuizAttempts)
                .SelectMany(qa => qa.QuizAnswers)
                .ToList();
            if (quizAnswers.Count > 0)
            {
                _context.QuizAnswers.RemoveRange(quizAnswers);
            }

            var quizAttempts = existing.Quizzes
                .SelectMany(q => q.QuizAttempts)
                .ToList();
            if (quizAttempts.Count > 0)
            {
                _context.QuizAttempts.RemoveRange(quizAttempts);
            }

            var questions = existing.Quizzes
                .SelectMany(q => q.Questions)
                .ToList();
            if (questions.Count > 0)
            {
                _context.Questions.RemoveRange(questions);
            }

            if (existing.Quizzes.Count > 0)
            {
                _context.Quizzes.RemoveRange(existing.Quizzes);
            }

            var assignmentSubmissions = existing.Assignments
                .SelectMany(a => a.AssignmentSubmissions)
                .ToList();
            if (assignmentSubmissions.Count > 0)
            {
                _context.AssignmentSubmissions.RemoveRange(assignmentSubmissions);
            }

            if (existing.Assignments.Count > 0)
            {
                _context.Assignments.RemoveRange(existing.Assignments);
            }

            if (existing.LessonProgresses.Count > 0)
            {
                _context.LessonProgresses.RemoveRange(existing.LessonProgresses);
            }

            _context.Lessons.Remove(existing);

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

        public async Task<bool> ExistsOrderIndexAsync(int sectionId, int orderIndex, int? excludeLessonId = null)
        {
            return await _context.Lessons
                .AnyAsync(l => l.SectionId == sectionId
                               && !l.IsDeleted
                               && l.OrderIndex == orderIndex
                               && (!excludeLessonId.HasValue || l.LessonId != excludeLessonId.Value));
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

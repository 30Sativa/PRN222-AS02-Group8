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
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetByIdAsync(int quizId)
        {
            return await _context.Quizzes
                .Include(q => q.Lesson)              
                .ThenInclude(l => l.Section)    
                .Include(q => q.Questions)            
                .FirstOrDefaultAsync(q => q.QuizId == quizId);
        }

        public async Task<IEnumerable<Quiz>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Quizzes
                .Where(q => q.LessonId == lessonId)
                .ToListAsync();
        }

        public async Task AddAsync(Quiz quiz) => await _context.Quizzes.AddAsync(quiz);

        public void Update(Quiz quiz) => _context.Quizzes.Update(quiz);

        public void Delete(Quiz quiz) => _context.Quizzes.Remove(quiz);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}


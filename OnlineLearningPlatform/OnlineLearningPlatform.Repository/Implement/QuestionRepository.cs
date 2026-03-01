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
    public class QuestionRepository : IQuestionRepository
    {
        private readonly ApplicationDbContext _context;

        public QuestionRepository(ApplicationDbContext context) => _context = context;

        public async Task<Question?> GetByIdAsync(int id) => await _context.Questions.FindAsync(id);

        public async Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId)
            => await _context.Questions.Where(q => q.QuizId == quizId).ToListAsync();

        public async Task AddAsync(Question question) => await _context.Questions.AddAsync(question);

        public void Update(Question question) => _context.Questions.Update(question);

        public void Delete(int id)
        {
            var item = _context.Questions.Find(id);
            if (item != null) _context.Questions.Remove(item);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

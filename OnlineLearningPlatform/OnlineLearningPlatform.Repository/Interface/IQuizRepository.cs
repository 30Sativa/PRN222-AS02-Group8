using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IQuizRepository
    {
        Task<Quiz?> GetByIdAsync(int quizId);
        Task<IEnumerable<Quiz>> GetByLessonIdAsync(int lessonId);
        Task AddAsync(Quiz quiz);
        void Update(Quiz quiz);
        void Delete(Quiz quiz);
        Task SaveChangesAsync();
    }
}

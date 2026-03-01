using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(int questionId);
        Task<IEnumerable<Question>> GetByQuizIdAsync(int quizId);
        Task AddAsync(Question question);
        void Update(Question question);
        void Delete(int questionId);
        Task SaveChangesAsync();
    }
}

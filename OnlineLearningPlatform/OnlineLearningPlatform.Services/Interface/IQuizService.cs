using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IQuizService
    {
        // Cho Instructor (Quản lý)
        Task<Quiz?> GetQuizDetailsAsync(int quizId);
        Task CreateQuizAsync(Quiz quiz);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(int quizId);
        Task AddQuestionAsync(Question question);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(int questionId);
        Task<IEnumerable<OnlineLearningPlatform.Models.Entities.Course>> GetCoursesForInstructorAsync();
        Task<IEnumerable<OnlineLearningPlatform.Models.Entities.Quiz>> GetQuizzesByLessonAsync(int lessonId);




        // Cho Student (Làm bài)
        Task<bool> CanStudentAttemptQuizAsync(string userId, int quizId, int? maxAttempts);
        Task<QuizAttempt> SubmitQuizAsync(QuizSubmissionDto submission);
        Task<QuizAttempt?> GetAttemptResultAsync(Guid attemptId);
        Task<IEnumerable<QuizAttempt>> GetStudentHistoryAsync(string userId, int quizId);
    }
}

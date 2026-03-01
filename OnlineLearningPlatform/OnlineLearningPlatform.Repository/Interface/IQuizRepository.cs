using OnlineLearningPlatform.Models.Entities;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(int id);
    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId);
    Task<Quiz?> GetFullQuizForDeleteAsync(int quizId); 
    Task<IEnumerable<Quiz>> GetByLessonAsync(int lessonId);
    Task AddAsync(Quiz quiz);
    void Update(Quiz quiz);
    void Delete(Quiz quiz);
    void DeleteRangeQuestions(IEnumerable<Question> questions);
    void DeleteRangeAttempts(IEnumerable<QuizAttempt> attempts);
    void DeleteRangeAnswers(IEnumerable<QuizAnswer> answers);
    Task SaveChangesAsync();
}
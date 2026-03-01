using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.DTOs.Quiz
{
    public class QuizSubmissionDto
    {
        public int QuizId { get; set; }
        public string UserId { get; set; } = default!;
        public List<QuestionAnswerDto> Answers { get; set; } = new();
    }
}

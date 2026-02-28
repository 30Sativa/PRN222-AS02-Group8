using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.DTOs.Quiz
{
    public class QuestionAnswerDto
    {
        public int QuestionId { get; set; }
        public string UserSelectedAnswer { get; set; } = default!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.DTOs.Auth.Response
{
    public class AuthResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}

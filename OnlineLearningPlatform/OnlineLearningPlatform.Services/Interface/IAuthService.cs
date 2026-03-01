using OnlineLearningPlatform.Services.DTOs.Auth.Request;
using OnlineLearningPlatform.Services.DTOs.Auth.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> ConfirmEmailAsync(string userId, string token);
        Task LogoutAsync();
    }
}

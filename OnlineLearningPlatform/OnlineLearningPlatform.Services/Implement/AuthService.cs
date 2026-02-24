using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Auth.Request;
using OnlineLearningPlatform.Services.DTOs.Auth.Response;
using OnlineLearningPlatform.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;

        public AuthService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var result =  await _userRepo.PasswordSignInAsync(request.Email, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }
            return new AuthResult
            {
                Success = true,
                Message = "Login successful."
            };
        }

        public async Task LogoutAsync()
        {
            await _userRepo.SignOutAsync();
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email already in use."
                };
            }
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
            };
            var result = await _userRepo.CreateUserAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            await _userRepo.AddToRoleAsync(user, "Student");

            return new AuthResult
            {
                Success = true,
                Message = "Registration successful."
            };
        }
    }
}

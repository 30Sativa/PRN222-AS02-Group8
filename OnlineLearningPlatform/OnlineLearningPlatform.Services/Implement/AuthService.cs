using Microsoft.Extensions.Options;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Auth.Request;
using OnlineLearningPlatform.Services.DTOs.Auth.Response;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.Settings;
using System.Net;

namespace OnlineLearningPlatform.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly AppSettings _appSettings;

        public AuthService(
            IUserRepository userRepo,
            IEmailService emailService,
            IOptions<AppSettings> appSettings)
        {
            _userRepo = userRepo;
            _emailService = emailService;
            _appSettings = appSettings.Value;
        }

        // ================= REGISTER =================
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
                FullName = request.FullName
            };

            var createResult = await _userRepo.CreateUserAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
                };
            }

             await _userRepo.AddToRoleAsync(user, Roles.Student);

            

            // Generate email confirmation token
            var token = await _userRepo.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);

            var confirmationLink =
                $"{_appSettings.BaseUrl}/Auth/ConfirmEmail?userId={user.Id}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Confirm your account",
                $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>");

            return new AuthResult
            {
                Success = true,
                Message = "Registration successful. Please check your email to confirm your account."
            };
        }

        // ================= CONFIRM EMAIL =================
        public async Task<AuthResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var result = await _userRepo.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid or expired token."
                };
            }

            return new AuthResult
            {
                Success = true,
                Message = "Email confirmed successfully."
            };
        }

        // ================= LOGIN =================
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!user.EmailConfirmed)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Please confirm your email before logging in."
                };
            }

            var signInResult = await _userRepo.PasswordSignInAsync(request.Email, request.Password);

            if (!signInResult.Succeeded)
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

        // ================= LOGOUT =================
        public async Task LogoutAsync()
        {
            await _userRepo.SignOutAsync();
        }
    }
}
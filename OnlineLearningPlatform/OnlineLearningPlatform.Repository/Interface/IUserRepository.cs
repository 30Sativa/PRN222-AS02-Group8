using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        // FIX: đổi thành nullable vì email có thể ko tồn tại trong DB
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<SignInResult> PasswordSignInAsync(string email, string password);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task SignOutAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task UpdateUserAsync(ApplicationUser user);
        // Lấy danh sách role của 1 user (để hiển thị trên UI)
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    }
}

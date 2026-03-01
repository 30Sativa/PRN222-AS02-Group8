using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);

        // Nullable vì user có thể không tồn tại trong DB
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        Task<SignInResult> PasswordSignInAsync(string email, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task SignOutAsync();

        Task<List<ApplicationUser>> GetAllUsersAsync();
        Task UpdateUserAsync(ApplicationUser user);
        // Lấy role của 1 user
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);


        // Search user đa tiêu chí trên DB (keyword tìm theo tên/email/sđt)
        Task<List<ApplicationUser>> SearchUsersAsync(string? keyword);

        // Xóa user khỏi DB
        Task<IdentityResult> DeleteUserAsync(ApplicationUser user);

        // Xóa role cũ, gán role mới
        Task RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles);

        // Lấy tổng số users (cho Dashboard stats)
        Task<int> CountUsersAsync();

        // Lấy all users đang có role cụ thể
        Task<IList<ApplicationUser>> GetUsersInRoleAsync(string role);
    }
}

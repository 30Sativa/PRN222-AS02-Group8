using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task AddToRoleAsync(ApplicationUser user, string role)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<SignInResult> PasswordSignInAsync(string email, string password)
        {
            return await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        // Lấy role của 1 user (VD: "Admin", "Student")
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        // Search đa tiêu chí trên DB - query EF Core, ko lọc trên FE
        public async Task<List<ApplicationUser>> SearchUsersAsync(string? keyword)
        {
            var query = _userManager.Users.AsQueryable();

            // Lọc theo keyword: tìm trong FullName, Email, PhoneNumber
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var term = keyword.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term))
                );
            }

            // Sắp xếp theo tên cho dễ nhìn
            return await query.OrderBy(u => u.FullName).ToListAsync();
        }

        // Xóa user khỏi DB
        public async Task<IdentityResult> DeleteUserAsync(ApplicationUser user)
        {
            return await _userManager.DeleteAsync(user);
        }

        // Gỡ tất cả role cũ của user (dùng khi đổi role)
        public async Task RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            await _userManager.RemoveFromRolesAsync(user, roles);
        }

        // Đếm tổng users (cho Dashboard)
        public async Task<int> CountUsersAsync()
        {
            return await _userManager.Users.CountAsync();
        }

        // Lấy all users thuộc 1 role cụ thể
        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string role)
        {
            return await _userManager.GetUsersInRoleAsync(role);
        }
    }
}

using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // Lấy tất cả users kèm role
        public async Task<List<UserInListResponse>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return await MapUsersToResponseAsync(users);
        }

        // Search đa tiêu chí từ BE + phân trang
        public async Task<(List<UserInListResponse> Users, int TotalCount)> SearchUsersAsync(SearchUserRequest request)
        {
            // Bước 1: query DB theo keyword (tên, email, sđt)
            var users = await _userRepo.SearchUsersAsync(request.Keyword);

            // Bước 2: map sang DTO kèm role
            var mapped = await MapUsersToResponseAsync(users);

            // Bước 3: lọc theo role (phải lọc sau khi lấy role vì Identity ko JOIN được trực tiếp)
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                mapped = mapped.Where(u =>
                    u.Role.Equals(request.Role, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            // Bước 4: phân trang
            var totalCount = mapped.Count;
            var pagedUsers = mapped
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return (pagedUsers, totalCount);
        }

        // Lấy thông tin chi tiết 1 user
        public async Task<UserInfoResponse?> GetUserInfoAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userRepo.GetUserRolesAsync(user);

            return new UserInfoResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                Role = roles.FirstOrDefault() ?? "Student"
            };
        }

        // Cập nhật user, validate input trước
        public async Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request)
        {
            var userToUpdate = await _userRepo.GetUserByIdAsync(userId);
            if (userToUpdate == null) return false;

            // Chỉ cập nhật nếu giá trị mới ko rỗng/whitespace
            if (!string.IsNullOrWhiteSpace(request.FullName))
                userToUpdate.FullName = request.FullName.Trim();

            if (request.Bio != null)
                userToUpdate.Bio = request.Bio.Trim();

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                userToUpdate.PhoneNumber = request.PhoneNumber.Trim();

            await _userRepo.UpdateUserAsync(userToUpdate);
            return true;
        }

        // Admin xóa user
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return false;

            var result = await _userRepo.DeleteUserAsync(user);
            return result.Succeeded;
        }

        // Admin đổi role: xóa role cũ → gán role mới
        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return false;

            // Kiểm tra role mới hợp lệ
            var validRoles = new[] { "Admin", "Teacher", "Student" };
            if (!validRoles.Contains(newRole)) return false;

            // Xóa tất cả role cũ
            var currentRoles = await _userRepo.GetUserRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userRepo.RemoveFromRolesAsync(user, currentRoles);
            }

            // Gán role mới
            await _userRepo.AddToRoleAsync(user, newRole);
            return true;
        }

        // Đếm user theo role
        public async Task<int> CountUsersByRoleAsync(string role)
        {
            var users = await _userRepo.GetUsersInRoleAsync(role);
            return users.Count;
        }

        // Đếm tổng users
        public async Task<int> CountAllUsersAsync()
        {
            return await _userRepo.CountUsersAsync();
        }

        // Helper: map list entity → list DTO kèm role
        private async Task<List<UserInListResponse>> MapUsersToResponseAsync(IEnumerable<OnlineLearningPlatform.Models.Entities.Identity.ApplicationUser> users)
        {
            var response = new List<UserInListResponse>();
            foreach (var u in users)
            {
                var roles = await _userRepo.GetUserRolesAsync(u);
                response.Add(new UserInListResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = roles.FirstOrDefault() ?? "Student"
                });
            }
            return response;
        }
    }
}

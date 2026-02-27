using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.DTOs.User.Response;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IUserService
    {
        // Lấy danh sách tất cả users
        Task<List<UserInListResponse>> GetAllUsersAsync();

        // Search đa tiêu chí + phân trang (query trên DB)
        Task<(List<UserInListResponse> Users, int TotalCount)> SearchUsersAsync(SearchUserRequest request);

        // Lấy thông tin chi tiết 1 user (return null nếu ko tìm thấy)
        Task<UserInfoResponse?> GetUserInfoAsync(string userId);

        // Cập nhật thông tin user, return true nếu thành công
        Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request);

        // Admin xóa user, return true nếu thành công
        Task<bool> DeleteUserAsync(string userId);

        // Admin đổi role cho user, return true nếu thành công
        Task<bool> ChangeUserRoleAsync(string userId, string newRole);

        // Đếm user theo role (cho Dashboard stats)
        Task<int> CountUsersByRoleAsync(string role);

        // Đếm tổng users
        Task<int> CountAllUsersAsync();
    }
}

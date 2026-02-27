using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.DTOs.User.Response;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IUserService
    {
        // Admin: lấy danh sách tất cả users
        Task<List<UserInListResponse>> GetAllUsersAsync();
        // Lấy thông tin chi tiết 1 user (return null nếu ko tìm thấy)
        Task<UserInfoResponse?> GetUserInfoAsync(string userId);
        // Cập nhật thông tin user, return true nếu thành công
        Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request);
    }
}

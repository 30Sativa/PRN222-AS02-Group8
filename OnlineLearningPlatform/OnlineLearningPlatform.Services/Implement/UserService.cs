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

        // Lấy danh sách all users kèm role (cho Admin quản lý)
        public async Task<List<UserInListResponse>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();

            var response = new List<UserInListResponse>();
            foreach (var u in users)
            {
                // Lấy role từ Identity (VD: "Admin", "Student", "Teacher")
                var roles = await _userRepo.GetUserRolesAsync(u);
                response.Add(new UserInListResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    // Lấy role đầu tiên, mặc định "Student" nếu chưa gán role
                    Role = roles.FirstOrDefault() ?? "Student"
                });
            }

            return response;
        }

        // Lấy thông tin chi tiết 1 user
        // FIX: return null thay vì throw Exception → UI dễ xử lý hơn
        public async Task<UserInfoResponse?> GetUserInfoAsync(string userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);

            // Trả null nếu ko tìm thấy → PageModel sẽ hiển thị NotFound
            if (user == null) return null;

            var roles = await _userRepo.GetUserRolesAsync(user);

            return new UserInfoResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                // Thêm Bio và CreatedAt cho trang Profile
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                Role = roles.FirstOrDefault() ?? "Student"
            };
        }

        // Cập nhật thông tin user
        // FIX: return bool + validate input trước khi update
        public async Task<bool> UpdateUserInfoAsync(string userId, UpdateUserRequest request)
        {
            var userToUpdate = await _userRepo.GetUserByIdAsync(userId);

            // User ko tồn tại → return false
            if (userToUpdate == null) return false;

            // FIX: chỉ cập nhật nếu giá trị mới ko rỗng/whitespace
            if (!string.IsNullOrWhiteSpace(request.FullName))
                userToUpdate.FullName = request.FullName.Trim();

            if (request.Bio != null)
                userToUpdate.Bio = request.Bio.Trim();

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                userToUpdate.PhoneNumber = request.PhoneNumber.Trim();

            await _userRepo.UpdateUserAsync(userToUpdate);
            return true;
        }
    }
}

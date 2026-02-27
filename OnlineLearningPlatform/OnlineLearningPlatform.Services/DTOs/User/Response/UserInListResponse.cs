namespace OnlineLearningPlatform.Services.DTOs.User.Response
{
    // DTO dùng cho danh sách users (Admin xem list)
    public class UserInListResponse
    {
        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        // Thêm Role để Admin biết ai là Student/Teacher/Admin
        public string Role { get; set; } = "Student";
    }
}

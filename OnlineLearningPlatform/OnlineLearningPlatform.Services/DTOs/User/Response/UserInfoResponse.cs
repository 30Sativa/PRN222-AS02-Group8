namespace OnlineLearningPlatform.Services.DTOs.User.Response
{
    // Kế thừa UserInListResponse + thêm các field chi tiết cho trang Profile
    public class UserInfoResponse : UserInListResponse
    {
        // Giới thiệu bản thân (Teacher hay dùng)
        public string? Bio { get; set; }

        // Ngày tạo tài khoản, hiển thị trên trang profile
        public DateTime CreatedAt { get; set; }
    }
}

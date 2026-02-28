namespace OnlineLearningPlatform.Services.DTOs.Enrollment
{
    public class EnrollmentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static EnrollmentResult Ok(string message = "Thành công")
            => new() { Success = true, Message = message };

        public static EnrollmentResult Fail(string message)
            => new() { Success = false, Message = message };
    }
}

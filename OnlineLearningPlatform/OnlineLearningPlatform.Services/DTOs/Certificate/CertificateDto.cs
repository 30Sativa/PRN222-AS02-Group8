namespace OnlineLearningPlatform.Services.DTOs.Certificate
{
    public class CertificateDto
    {
        public Guid CertificateId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string CertificateCode { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
    }

    public class IssueCertificateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CertificateDto? Certificate { get; set; }
        public bool AlreadyIssued { get; set; }

        public static IssueCertificateResult Ok(CertificateDto cert)
            => new() { Success = true, Message = "Chứng chỉ đã được cấp.", Certificate = cert };

        public static IssueCertificateResult Already()
            => new() { Success = true, AlreadyIssued = true, Message = "Chứng chỉ đã được cấp trước đó." };

        public static IssueCertificateResult Fail(string message)
            => new() { Success = false, Message = message };
    }
}

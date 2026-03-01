namespace OnlineLearningPlatform.Services.DTOs.Payment
{
    public class CheckoutRequestDto
    {
        public Guid CourseId { get; set; }
        public string PaymentMethod { get; set; } = "VNPAY"; 
    }

    public class PaymentUrlResponseDto
    {
        public string Url { get; set; } = default!;
    }
}

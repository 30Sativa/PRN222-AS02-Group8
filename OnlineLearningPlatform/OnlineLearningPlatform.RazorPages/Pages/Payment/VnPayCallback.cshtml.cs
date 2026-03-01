using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Payment
{
    public class VnPayCallbackModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public VnPayCallbackModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _paymentService.ProcessVnPayCallbackAsync(Request.Query);
                if (result.OrderId.HasValue)
                {
                    return RedirectToPage("/Payment/PaymentResult", new { orderId = result.OrderId.Value });
                }
                else
                {
                    ErrorMessage = result.Message;
                    IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                ErrorMessage = "Có lỗi xảy ra trong quá trình xử lý giao dịch: " + ex.Message;
            }

            return Page(); // Fallback if no orderId
        }
    }
}

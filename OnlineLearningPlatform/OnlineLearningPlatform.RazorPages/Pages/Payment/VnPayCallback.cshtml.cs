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
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                IsSuccess = await _paymentService.ProcessVnPayCallbackAsync(Request.Query);

                if (IsSuccess)
                {
                    TransactionId = Request.Query["vnp_TransactionNo"];
                }
                else
                {
                    ErrorMessage = "Chữ ký không hợp lệ hoặc thanh toán bị hủy bỏ từ ứng dụng VNPAY.";
                }
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                ErrorMessage = "Có lỗi xảy ra trong quá trình xử lý giao dịch: " + ex.Message;
            }

            return Page();
        }
    }
}

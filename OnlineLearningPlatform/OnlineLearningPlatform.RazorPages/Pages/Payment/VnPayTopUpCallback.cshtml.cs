using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Payment
{
    public class VnPayTopUpCallbackModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public VnPayTopUpCallbackModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? Amount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var result = await _paymentService.ProcessTopUpCallbackAsync(Request.Query);
                if (result.OrderId.HasValue)
                {
                    if (result.IsSuccess)
                    {
                        IsSuccess = true;
                        return RedirectToPage("/Wallet/TopUp", new { area = "Student", success = true });
                    }
                    else
                    {
                        ErrorMessage = result.Message;
                        return RedirectToPage("/Wallet/TopUp", new { area = "Student", error = result.Message });
                    }
                }
                else
                {
                    ErrorMessage = result.Message;
                    return RedirectToPage("/Wallet/TopUp", new { area = "Student", error = result.Message });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Có lỗi xảy ra trong quá trình xử lý: " + ex.Message;
                return RedirectToPage("/Wallet/TopUp", new { area = "Student", error = ErrorMessage });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Wallet
{
    [Authorize(Roles = "Student")]
    public class TopUpModel : PageModel
    {
        private readonly IWalletService _walletService;

        public TopUpModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public Models.Entities.Wallet StudentWallet { get; set; } = null!;
        public string? PaymentUrl { get; set; }
        public int? TopUpOrderId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public decimal Amount { get; set; } = 50000;

        public static readonly decimal[] QuickAmounts = { 50000, 100000, 200000, 500000, 1000000 };

        public async Task OnGetAsync(string? error = null, bool success = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                StudentWallet = await _walletService.GetOrCreateWalletAsync(userId);
            }

            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error;
            }

            if (success)
            {
                SuccessMessage = "Nạp tiền thành công! Số dư đã được cập nhật.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (Amount < 10000)
            {
                ErrorMessage = "Số tiền nạp tối thiểu là 10.000 VNĐ.";
                await OnGetAsync();
                return Page();
            }

            if (Amount > 100000000)
            {
                ErrorMessage = "Số tiền nạp tối đa là 100.000.000 VNĐ.";
                await OnGetAsync();
                return Page();
            }

            StudentWallet = await _walletService.GetOrCreateWalletAsync(userId);
            return Page();
        }
    }
}

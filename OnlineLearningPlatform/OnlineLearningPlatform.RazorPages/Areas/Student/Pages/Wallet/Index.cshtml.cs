using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Wallet
{
    [Authorize(Roles = "Student")]
    public class IndexModel : PageModel
    {
        private readonly IWalletService _walletService;

        public IndexModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public Models.Entities.Wallet StudentWallet { get; set; } = null!;
        public List<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                StudentWallet = await _walletService.GetOrCreateWalletAsync(userId);
                Transactions = await _walletService.GetTransactionHistoryAsync(userId);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.ViewComponents
{
    public class WalletBalanceViewComponent : ViewComponent
    {
        private readonly IWalletService _walletService;

        public WalletBalanceViewComponent(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Only render for authenticated students
            if (!UserClaimsPrincipal.Identity?.IsAuthenticated ?? true)
                return Content(string.Empty);

            if (!UserClaimsPrincipal.IsInRole("Student"))
                return Content(string.Empty);

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Content(string.Empty);

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);
            return View(wallet.Balance);
        }
    }
}

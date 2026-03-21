using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Coupons
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICouponService _couponService;

        public IndexModel(ICouponService couponService)
        {
            _couponService = couponService;
        }

        public List<Coupon> Coupons { get; set; } = new();

        public async Task OnGetAsync(bool includeExpired = false)
        {
            Coupons = await _couponService.GetAllCouponsAsync(includeExpired);
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int couponId)
        {
            var result = await _couponService.DeactivateCouponAsync(couponId);
            if (!result)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã khuyến mãi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã vô hiệu hóa mã khuyến mãi.";
            }
            return RedirectToPage();
        }
    }
}

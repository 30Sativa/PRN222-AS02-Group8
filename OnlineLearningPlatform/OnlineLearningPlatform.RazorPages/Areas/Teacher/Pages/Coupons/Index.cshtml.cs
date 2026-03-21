using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Coupons
{
    [Authorize(Roles = "Teacher")]
    public class IndexModel : PageModel
    {
        private readonly ICouponService _couponService;

        public IndexModel(ICouponService couponService)
        {
            _couponService = couponService;
        }

        public List<Coupon> Coupons { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (teacherId != null)
            {
                Coupons = await _couponService.GetCouponsByTeacherAsync(teacherId);
            }
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int couponId)
        {
            await _couponService.DeactivateCouponAsync(couponId);
            TempData["SuccessMessage"] = "Đã vô hiệu hóa mã khuyến mãi.";
            return RedirectToPage();
        }
    }
}

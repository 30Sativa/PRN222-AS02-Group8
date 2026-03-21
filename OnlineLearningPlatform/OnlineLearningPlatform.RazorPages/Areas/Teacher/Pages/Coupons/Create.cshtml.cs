using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Coupons
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ICouponService _couponService;
        private readonly ICourseService _courseService;

        public CreateModel(ICouponService couponService, ICourseService courseService)
        {
            _couponService = couponService;
            _courseService = courseService;
        }

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public CouponDiscountType DiscountType { get; set; } = CouponDiscountType.Percentage;

        [BindProperty]
        public decimal DiscountValue { get; set; } = 10;

        [BindProperty]
        public decimal? MaxDiscountAmount { get; set; }

        [BindProperty]
        public bool LimitMaxDiscount { get; set; }

        [BindProperty]
        public int? UsageLimit { get; set; }

        [BindProperty]
        public int? MaxUsagePerUser { get; set; } = 1;

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        [BindProperty]
        public decimal? MinOrderAmount { get; set; }

        [BindProperty]
        public bool IsCourseSpecific { get; set; } = true;

        [BindProperty]
        public Guid? CourseId { get; set; }

        public List<Course> MyCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (teacherId != null)
            {
                MyCourses = await _courseService.GetMyCoursesAsync(teacherId);
            }
        }

        private void ApplyMaxDiscountBinding()
        {
            if (DiscountType != CouponDiscountType.Percentage || !LimitMaxDiscount)
                MaxDiscountAmount = null;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (teacherId == null) return RedirectToPage("/Auth/Login");

            ApplyMaxDiscountBinding();

            if (string.IsNullOrWhiteSpace(Code))
            {
                ModelState.AddModelError(string.Empty, "Mã coupon không được để trống.");
                await OnGetAsync();
                return Page();
            }

            if (DiscountValue <= 0)
            {
                ModelState.AddModelError(string.Empty, "Giá trị giảm phải lớn hơn 0.");
                await OnGetAsync();
                return Page();
            }

            if (DiscountType == CouponDiscountType.Percentage && DiscountValue > 100)
            {
                ModelState.AddModelError(string.Empty, "Phần trăm giảm không thể lớn hơn 100.");
                await OnGetAsync();
                return Page();
            }

            if (DiscountType == CouponDiscountType.Percentage && LimitMaxDiscount &&
                (!MaxDiscountAmount.HasValue || MaxDiscountAmount <= 0))
            {
                ModelState.AddModelError(nameof(MaxDiscountAmount), "Nhập mức trần giảm (VNĐ) hoặc bỏ chọn giới hạn.");
                await OnGetAsync();
                return Page();
            }

            if (EndDate <= StartDate)
            {
                ModelState.AddModelError(string.Empty, "Ngày kết thúc phải sau ngày bắt đầu.");
                await OnGetAsync();
                return Page();
            }

            if (IsCourseSpecific && CourseId == null)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn khóa học khi áp dụng cho khóa cụ thể.");
                await OnGetAsync();
                return Page();
            }

            var coupon = new Coupon
            {
                Code = Code.Trim().ToUpper(),
                Description = Description,
                DiscountType = DiscountType,
                DiscountValue = DiscountValue,
                MaxDiscountAmount = MaxDiscountAmount,
                UsageLimit = UsageLimit,
                MaxUsagePerUser = MaxUsagePerUser,
                StartDate = StartDate,
                EndDate = EndDate,
                MinOrderAmount = MinOrderAmount,
                IsCourseSpecific = IsCourseSpecific,
                CourseId = IsCourseSpecific ? CourseId : null,
                Audience = CouponTarget.AllUsers,
                CreatedByTeacherId = teacherId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _couponService.CreateCouponAsync(coupon);
                TempData["SuccessMessage"] = $"Mã '{coupon.Code}' đã được tạo thành công.";
                return RedirectToPage("/Coupons/Index", new { area = "Teacher" });
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.Message.Contains("unique", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, $"Mã '{Code.ToUpper()}' đã tồn tại.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
                }
                await OnGetAsync();
                return Page();
            }
        }
    }
}

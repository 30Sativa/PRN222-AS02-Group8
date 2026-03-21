using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Coupons
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ICouponService _couponService;
        private readonly ICourseService _courseService;

        public EditModel(ICouponService couponService, ICourseService courseService)
        {
            _couponService = couponService;
            _courseService = courseService;
        }

        public List<SelectListItem> CourseOptions { get; set; } = new();

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

        /// <summary>
        /// Chỉ khi bật mới lưu MaxDiscountAmount (áp dụng cho loại %).
        /// </summary>
        [BindProperty]
        public bool LimitMaxDiscount { get; set; }

        [BindProperty]
        public int? UsageLimit { get; set; }

        [BindProperty]
        public int? MaxUsagePerUser { get; set; } = 1;

        [BindProperty]
        public DateTime StartDate { get; set; }

        [BindProperty]
        public DateTime EndDate { get; set; }

        [BindProperty]
        public decimal? MinOrderAmount { get; set; }

        [BindProperty]
        public bool IsCourseSpecific { get; set; }

        [BindProperty]
        public Guid? CourseId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();

            Code = coupon.Code;
            Description = coupon.Description;
            DiscountType = coupon.DiscountType;
            DiscountValue = coupon.DiscountValue;
            MaxDiscountAmount = coupon.MaxDiscountAmount;
            LimitMaxDiscount = coupon.MaxDiscountAmount.HasValue;
            UsageLimit = coupon.UsageLimit;
            MaxUsagePerUser = coupon.MaxUsagePerUser;
            StartDate = coupon.StartDate;
            EndDate = coupon.EndDate;
            MinOrderAmount = coupon.MinOrderAmount;
            IsCourseSpecific = coupon.IsCourseSpecific;
            CourseId = coupon.CourseId;

            await LoadCourseOptionsAsync();
            return Page();
        }

        private async Task LoadCourseOptionsAsync()
        {
            var courses = await _courseService.GetAllForAdminAsync();
            CourseOptions = courses
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.Title} · {c.CourseCode}"
                })
                .ToList();

            CourseOptions.Insert(0, new SelectListItem { Value = "", Text = "— Chọn khóa học —" });
        }

        private void ApplyMaxDiscountBinding()
        {
            if (DiscountType != CouponDiscountType.Percentage || !LimitMaxDiscount)
                MaxDiscountAmount = null;
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await LoadCourseOptionsAsync();

            var existing = await _couponService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            ApplyMaxDiscountBinding();

            if (string.IsNullOrWhiteSpace(Code))
            {
                ModelState.AddModelError(string.Empty, "Mã coupon không được để trống.");
                return Page();
            }

            if (DiscountValue <= 0)
            {
                ModelState.AddModelError(string.Empty, "Giá trị giảm phải lớn hơn 0.");
                return Page();
            }

            if (DiscountType == CouponDiscountType.Percentage && DiscountValue > 100)
            {
                ModelState.AddModelError(string.Empty, "Phần trăm giảm không thể lớn hơn 100.");
                return Page();
            }

            if (DiscountType == CouponDiscountType.Percentage && LimitMaxDiscount &&
                (!MaxDiscountAmount.HasValue || MaxDiscountAmount <= 0))
            {
                ModelState.AddModelError(nameof(MaxDiscountAmount), "Nhập mức trần giảm (VNĐ) hoặc bỏ chọn giới hạn.");
                return Page();
            }

            if (EndDate <= StartDate)
            {
                ModelState.AddModelError(string.Empty, "Ngày kết thúc phải sau ngày bắt đầu.");
                return Page();
            }

            if (IsCourseSpecific && (!CourseId.HasValue || CourseId == Guid.Empty))
            {
                ModelState.AddModelError(nameof(CourseId), "Vui lòng chọn khóa học khi bật áp dụng cho một khóa.");
                return Page();
            }

            if (UsageLimit.HasValue && UsageLimit.Value < existing.UsedCount)
            {
                ModelState.AddModelError(nameof(UsageLimit),
                    $"Tổng lượt dùng không thể nhỏ hơn số đã dùng ({existing.UsedCount}).");
                return Page();
            }

            existing.Code = Code.Trim().ToUpper();
            existing.Description = Description;
            existing.DiscountType = DiscountType;
            existing.DiscountValue = DiscountValue;
            existing.MaxDiscountAmount = MaxDiscountAmount;
            existing.UsageLimit = UsageLimit;
            existing.MaxUsagePerUser = MaxUsagePerUser;
            existing.StartDate = StartDate;
            existing.EndDate = EndDate;
            existing.MinOrderAmount = MinOrderAmount;
            existing.IsCourseSpecific = IsCourseSpecific;
            existing.CourseId = IsCourseSpecific ? CourseId : null;

            try
            {
                await _couponService.UpdateCouponAsync(existing);
                TempData["SuccessMessage"] = $"Đã cập nhật mã '{existing.Code}'.";
                return RedirectToPage("/Coupons/Index", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                    ex.Message.Contains("unique", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, $"Mã '{Code.ToUpper()}' đã tồn tại. Vui lòng chọn mã khác.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
                }
                return Page();
            }
        }
    }
}

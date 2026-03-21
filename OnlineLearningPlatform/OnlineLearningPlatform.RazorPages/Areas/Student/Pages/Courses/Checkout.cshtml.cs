using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Courses
{
    public class CheckoutModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IWalletService _walletService;
        private readonly ICouponService _couponService;

        public CheckoutModel(
            IStudentService studentService,
            IOrderService orderService,
            IPaymentService paymentService,
            IWalletService walletService,
            ICouponService couponService)
        {
            _studentService = studentService;
            _orderService = orderService;
            _paymentService = paymentService;
            _walletService = walletService;
            _couponService = couponService;
        }

        public StudentCourseResponse Course { get; set; } = null!;
        public decimal WalletBalance { get; set; } = 0;

        [BindProperty]
        public bool UseWallet { get; set; } = false;

        [BindProperty]
        public string? CouponCode { get; set; }

        public string? CouponMessage { get; set; }
        public bool CouponApplied { get; set; } = false;
        public decimal CouponDiscount { get; set; } = 0;
        public decimal FinalPrice { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync(Guid id, string? couponError = null, bool couponSuccess = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var course = await _studentService.GetCourseDetailAsync(id, userId);
            if (course == null) return NotFound();

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);
            WalletBalance = wallet.Balance;

            decimal price = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;
            FinalPrice = price;

            if (course.IsEnrolled)
                return RedirectToPage("/LearnCourse", new { courseId = id });

            if (!string.IsNullOrEmpty(couponError))
            {
                CouponMessage = couponError;
                CouponApplied = false;
            }
            else if (couponSuccess)
            {
                CouponMessage = "Áp dụng mã khuyến mãi thành công!";
                CouponApplied = true;
            }

            Course = course;
            return Page();
        }

        public async Task<IActionResult> OnPostApplyCouponAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var course = await _studentService.GetCourseDetailAsync(id, userId);
            if (course == null) return NotFound();

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);
            WalletBalance = wallet.Balance;

            decimal price = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;
            FinalPrice = price;

            if (string.IsNullOrWhiteSpace(CouponCode))
            {
                CouponMessage = "Vui lòng nhập mã khuyến mãi.";
                CouponApplied = false;
            }
            else
            {
                var (isValid, message, discount) = await _couponService.ValidateAndCalculateDiscountAsync(
                    CouponCode, userId, id, price);

                if (isValid)
                {
                    CouponDiscount = discount;
                    FinalPrice = Math.Max(0, price - discount);
                    CouponApplied = true;
                    CouponMessage = message;
                    TempData["CouponCode"] = CouponCode;
                    TempData["CouponDiscount"] = discount.ToString();
                    TempData["CouponFinalPrice"] = FinalPrice.ToString();
                }
                else
                {
                    CouponMessage = message;
                    CouponApplied = false;
                }
            }

            Course = course;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            var course = await _studentService.GetCourseDetailAsync(id, userId);
            if (course == null) return NotFound();

            decimal price = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;

            var couponCode = TempData["CouponCode"] as string;

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var (isValid, message, discount) = await _couponService.ValidateAndCalculateDiscountAsync(
                    couponCode, userId, id, price);
                if (!isValid)
                {
                    CouponMessage = message;
                    CouponApplied = false;
                    Course = course;
                    return Page();
                }
            }

            if (price > 0)
            {
                var order = await _orderService.CreateOrderAsync(userId, id, "VNPAY", UseWallet, couponCode);

                if (order.Status == OrderStatus.Completed)
                    return RedirectToPage("/Payment/PaymentResult", new { orderId = order.OrderId });

                var paymentUrl = _paymentService.CreateVnPayPaymentUrl(order, HttpContext);
                return Redirect(paymentUrl);
            }

            await _studentService.EnrollInCourseAsync(userId, id);
            return RedirectToPage("/LearnCourse", new { courseId = id });
        }
    }
}

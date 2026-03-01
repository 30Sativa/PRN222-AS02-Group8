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

        public CheckoutModel(IStudentService studentService, IOrderService orderService, IPaymentService paymentService, IWalletService walletService)
        {
            _studentService = studentService;
            _orderService = orderService;
            _paymentService = paymentService;
            _walletService = walletService;
        }

        public StudentCourseResponse Course { get; set; } = null!;
        public decimal WalletBalance { get; set; } = 0;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var course = await _studentService.GetCourseDetailAsync(id, userId);

            if (course == null)
            {
                return NotFound();
            }

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);
            WalletBalance = wallet.Balance;
            Course = course;

            // Nếu đã mua rồi thì redirect sang học
            if (Course.IsEnrolled)
            {
                return RedirectToPage("/LearnCourse", new { courseId = id });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id, bool UseWallet = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            var course = await _studentService.GetCourseDetailAsync(id, userId);
            if (course == null) return NotFound();

            decimal finalPrice = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;

            if (finalPrice > 0)
            {
                var order = await _orderService.CreateOrderAsync(userId, id, "VNPAY", UseWallet);

                if (order.Status == OrderStatus.Completed)
                {
                    return RedirectToPage("/Payment/PaymentResult", new { orderId = order.OrderId });
                }

                var paymentUrl = _paymentService.CreateVnPayPaymentUrl(order, HttpContext);
                return Redirect(paymentUrl);
            }

            await _studentService.EnrollInCourseAsync(userId, id);
            return RedirectToPage("/LearnCourse", new { courseId = id });
        }
    }
}

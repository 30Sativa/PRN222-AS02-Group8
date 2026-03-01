using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Courses
{
    public class DetailsModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public DetailsModel(IStudentService studentService, IOrderService orderService, IPaymentService paymentService)
        {
            _studentService = studentService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        public StudentCourseResponse Course { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _studentService.GetCourseDetailAsync(id, userId);

            if (course == null)
            {
                return NotFound();
            }

            Course = course;
            return Page();
        }

        public async Task<IActionResult> OnPostEnrollAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Navigate to login page
                return RedirectToPage("/Auth/Login");
            }

            var course = await _studentService.GetCourseDetailAsync(id, userId);
            if (course == null) return NotFound();

            decimal finalPrice = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;

            if (finalPrice > 0)
            {
                // Buy course logic
                var order = await _orderService.CreateOrderAsync(userId, id, "VNPAY");
                var paymentUrl = _paymentService.CreateVnPayPaymentUrl(order, HttpContext);
                return Redirect(paymentUrl);
            }

            // Free course logic
            await _studentService.EnrollInCourseAsync(userId, id);

            // Redirect to learning interface
            return RedirectToPage("/LearnCourse", new { courseId = id });
        }
    }
}

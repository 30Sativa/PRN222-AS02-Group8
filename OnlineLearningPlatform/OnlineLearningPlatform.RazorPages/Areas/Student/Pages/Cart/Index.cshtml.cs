using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Services;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly StudentCartService _cart;
        private readonly IStudentService _studentService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IWalletService _walletService;

        public IndexModel(
            StudentCartService cart,
            IStudentService studentService,
            IOrderService orderService,
            IPaymentService paymentService,
            IWalletService walletService)
        {
            _cart = cart;
            _studentService = studentService;
            _orderService = orderService;
            _paymentService = paymentService;
            _walletService = walletService;
        }

        public List<StudentCourseResponse> Items { get; private set; } = new();
        public decimal Total { get; set; }
        public decimal WalletBalance { get; set; }

        [BindProperty]
        public bool UseWallet { get; set; } = true;

        [BindProperty]
        public bool AgreeRefundPolicy { get; set; }

        public string? CheckoutError { get; set; }
        public string? RefundPolicyError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            await LoadCartAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            _cart.RemoveCourse(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAllAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Auth/Login");

            await LoadCartAsync(userId);

            if (Items.Count == 0)
            {
                CheckoutError = "Giỏ hàng trống.";
                return Page();
            }

            if (Total > 0 && !AgreeRefundPolicy)
            {
                RefundPolicyError = "Vui lòng đồng ý chính sách hoàn tiền trước khi thanh toán.";
                return Page();
            }

            var ids = Items.Select(i => i.CourseId).ToList();

            try
            {
                var order = await _orderService.CreateCartOrderAsync(userId, ids, UseWallet);
                _cart.Clear();

                if (order.Status == OrderStatus.Completed)
                    return RedirectToPage("/Payment/PaymentResult", new { orderId = order.OrderId });

                var paymentUrl = _paymentService.CreateVnPayPaymentUrl(order, HttpContext);
                return Redirect(paymentUrl);
            }
            catch (InvalidOperationException ex)
            {
                CheckoutError = ex.Message;
                return Page();
            }
        }

        private async Task LoadCartAsync(string userId)
        {
            Items = new List<StudentCourseResponse>();
            Total = 0;

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);
            WalletBalance = wallet.Balance;

            foreach (var id in _cart.GetCourseIds().ToList())
            {
                var c = await _studentService.GetCourseDetailAsync(id, userId);
                if (c == null || c.IsEnrolled)
                {
                    _cart.RemoveCourse(id);
                    continue;
                }

                Items.Add(c);
                var line = (c.DiscountPrice != null && c.DiscountPrice > 0) ? c.DiscountPrice.Value : c.Price;
                Total += line;
            }
        }
    }
}

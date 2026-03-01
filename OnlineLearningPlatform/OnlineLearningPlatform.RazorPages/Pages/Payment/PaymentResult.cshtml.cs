using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Pages.Payment
{
    public class PaymentResultModel : PageModel
    {
        private readonly IOrderService _orderService;

        public PaymentResultModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            Order = order;
            return Page();
        }
    }
}

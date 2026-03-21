using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize(Roles = "Student")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public WalletController(
            IWalletService walletService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _walletService = walletService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(new { success = false, error = "Người dùng chưa đăng nhập." });

            if (request.Amount < 10000)
                return BadRequest(new { success = false, error = "Số tiền nạp tối thiểu là 10.000 VNĐ." });

            if (request.Amount > 100000000)
                return BadRequest(new { success = false, error = "Số tiền nạp tối đa là 100.000.000 VNĐ." });

            try
            {
                var order = await _orderService.CreateTopUpOrderAsync(userId, request.Amount);

                var paymentUrl = _paymentService.CreateVnPayTopUpUrl(order, HttpContext);

                return Ok(new
                {
                    success = true,
                    paymentUrl = paymentUrl,
                    orderId = order.OrderId,
                    amount = request.Amount
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, error = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var wallet = await _walletService.GetOrCreateWalletAsync(userId);

            return Ok(new
            {
                success = true,
                balance = wallet.Balance,
                updatedAt = wallet.UpdatedAt
            });
        }
    }

    public class TopUpRequest
    {
        public decimal Amount { get; set; }
    }
}

using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IWalletService _walletService;

        public OrderService(IOrderRepository orderRepo, ICourseRepository courseRepo, IEnrollmentService enrollmentService, IWalletService walletService)
        {
            _orderRepo = orderRepo;
            _courseRepo = courseRepo;
            _enrollmentService = enrollmentService;
            _walletService = walletService;
        }

        public async Task<Order> CreateOrderAsync(string userId, Guid courseId, string paymentMethod, bool useWallet = false)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null) throw new Exception("Course not found");

            decimal finalPrice = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;

            decimal walletUsed = 0;
            if (useWallet)
            {
                var wallet = await _walletService.GetOrCreateWalletAsync(userId);
                if (wallet.Balance > 0)
                {
                    walletUsed = wallet.Balance >= finalPrice ? finalPrice : wallet.Balance;
                }
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = finalPrice,
                WalletUsed = walletUsed,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        CourseId = courseId,
                        Price = course.Price,
                        DiscountApplied = course.Price - finalPrice
                    }
                }
            };

            var createdOrder = await _orderRepo.CreateOrderAsync(order);

            if (walletUsed > 0)
            {
                await _walletService.DeductFundsAsync(userId, walletUsed, $"Thanh toán khóa học {course.Title}", createdOrder.OrderId);
            }

            if (walletUsed >= finalPrice)
            {
                // Fully paid by wallet, complete it
                createdOrder.Status = OrderStatus.Completed;
                createdOrder.PaymentMethod = "WALLET";
                createdOrder.CompletedAt = DateTime.UtcNow;
                await _orderRepo.UpdateOrderAsync(createdOrder);
                await _enrollmentService.EnrollAsync(userId, courseId, true);
            }

            return createdOrder;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepo.GetOrderByIdAsync(orderId);
        }

        public async Task<List<Order>> GetMyOrdersAsync(string userId)
        {
            return await _orderRepo.GetOrdersByUserIdAsync(userId);
        }

        public async Task<bool> CompleteOrderAsync(int orderId, string transactionId, string gatewayResponse)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Completed) return false;

            order.Status = OrderStatus.Completed;
            order.TransactionId = transactionId;
            order.PaymentGatewayResponse = gatewayResponse;
            order.CompletedAt = DateTime.UtcNow;

            var result = await _orderRepo.UpdateOrderAsync(order);
            if (result)
            {
                // Gọi với forceEnroll = true để bypass check giá tiền
                // Assuming an order detail always exists and we enroll for the first course in the order
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    await _enrollmentService.EnrollAsync(order.UserId, order.OrderDetails.First().CourseId, true);
                }
            }

            return result;
        }

        public async Task<bool> FailOrderAsync(int orderId, string gatewayResponse)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Pending) return false;

            order.Status = OrderStatus.Failed;
            order.PaymentGatewayResponse = gatewayResponse;

            var result = await _orderRepo.UpdateOrderAsync(order);

            // Hoàn lại tiền vào ví nếu đơn hàng bị lỗi (ví dụ huỷ thanh toán VNPAY)
            if (result && order.WalletUsed > 0)
            {
                await _walletService.AddFundsAsync(
                    order.UserId, 
                    order.WalletUsed, 
                    $"Hoàn tiền do hủy giao dịch mua khóa học", 
                    order.OrderId, 
                    Models.Entities.WalletTransactionType.Refund
                );
            }

            return result;
        }

        public async Task<(bool Success, string Message)> RefundCourseToWalletAsync(string userId, Guid courseId)
        {
            var myOrders = await _orderRepo.GetOrdersByUserIdAsync(userId);
            // Tìm đơn hàng thành công gần nhất cho course này
            var order = myOrders.OrderByDescending(o => o.CreatedAt)
                                .FirstOrDefault(o => o.Status == OrderStatus.Completed && 
                                                     o.OrderDetails.Any(od => od.CourseId == courseId));

            if (order == null)
            {
                return (false, "Không tìm thấy giao dịch mua khóa học này.");
            }

            // Kiểm tra điều kiện 7 ngày
            if (order.CompletedAt.HasValue && (DateTime.UtcNow - order.CompletedAt.Value).TotalDays > 7)
            {
                return (false, "Đã quá thời hạn 7 ngày để yêu cầu hoàn tiền.");
            }

            // Thực hiện hoàn tiền toàn bộ giá trị đơn hàng (bao gồm tiền VNPAY và tiền ví)
            var totalRefund = order.TotalAmount; // Vì TotalAmount = VNPAY_paid + WalletUsed (và giá sau discount)
            
            var addFundsSuccess = await _walletService.AddFundsAsync(
                userId, 
                totalRefund, 
                $"Hoàn tiền khóa học {order.OrderDetails.First().Course.Title}", 
                order.OrderId,
                Models.Entities.WalletTransactionType.Refund
            );
            if (!addFundsSuccess)
            {
                return (false, "Lỗi khi cộng tiền vào ví.");
            }

            // Cập nhật trạng thái Order
            order.Status = OrderStatus.Refunded;
            await _orderRepo.UpdateOrderAsync(order);

            // Hủy ghi danh
            await _enrollmentService.CancelEnrollmentAsync(userId, courseId);

            return (true, "Đã hoàn tiền thành công vào ví của bạn.");
        }
    }
}

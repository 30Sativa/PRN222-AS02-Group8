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
        private readonly IProgressService _progressService;
        private readonly ICouponService _couponService;

        public OrderService(
            IOrderRepository orderRepo,
            ICourseRepository courseRepo,
            IEnrollmentService enrollmentService,
            IWalletService walletService,
            IProgressService progressService,
            ICouponService couponService)
        {
            _orderRepo = orderRepo;
            _courseRepo = courseRepo;
            _enrollmentService = enrollmentService;
            _walletService = walletService;
            _progressService = progressService;
            _couponService = couponService;
        }

        public async Task<Order> CreateOrderAsync(string userId, Guid courseId, string paymentMethod, bool useWallet = false, string? couponCode = null)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null) throw new Exception("Course not found");

            decimal price = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;
            decimal? couponDiscountAmount = null;
            int? appliedCouponId = null;

            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var (isValid, _, discountAmount) = await _couponService.ValidateAndCalculateDiscountAsync(
                    couponCode, userId, courseId, price);

                if (isValid && discountAmount > 0)
                {
                    couponDiscountAmount = discountAmount;
                    price = Math.Max(0, price - discountAmount);

                    var matchedCoupon = await _couponService.GetByCodeAsync(couponCode);
                    if (matchedCoupon != null) appliedCouponId = matchedCoupon.CouponId;
                }
            }

            decimal walletUsed = 0;
            if (useWallet && price > 0)
            {
                var wallet = await _walletService.GetOrCreateWalletAsync(userId);
                if (wallet.Balance > 0)
                {
                    walletUsed = wallet.Balance >= price ? price : wallet.Balance;
                }
            }

            decimal finalPrice = price;
            decimal courseOriginalPrice = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;
            decimal? orderDiscountApplied = couponDiscountAmount.HasValue ? couponDiscountAmount : null;

            var order = new Order
            {
                UserId = userId,
                TotalAmount = finalPrice,
                WalletUsed = walletUsed,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CouponId = appliedCouponId,
                CouponDiscountAmount = orderDiscountApplied,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        CourseId = courseId,
                        Price = courseOriginalPrice,
                        DiscountApplied = couponDiscountAmount ?? 0
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
                createdOrder.Status = OrderStatus.Completed;
                createdOrder.PaymentMethod = "WALLET";
                createdOrder.CompletedAt = DateTime.UtcNow;
                await _orderRepo.UpdateOrderAsync(createdOrder);
                await _enrollmentService.EnrollAsync(userId, courseId, true);

                if (appliedCouponId.HasValue)
                {
                    await _couponService.RecordUsageAsync(appliedCouponId.Value, userId, createdOrder.OrderId, couponDiscountAmount ?? 0);
                }
            }

            return createdOrder;
        }

        public async Task<Order> CreateCartOrderAsync(string userId, IReadOnlyList<Guid> courseIds, bool useWallet = false)
        {
            ArgumentNullException.ThrowIfNull(courseIds);
            var ids = courseIds.Distinct().ToList();
            if (ids.Count == 0)
                throw new InvalidOperationException("Giỏ hàng trống.");

            var orderDetails = new List<OrderDetail>();

            foreach (var courseId in ids)
            {
                if (await _enrollmentService.IsEnrolledAsync(userId, courseId))
                    continue;

                var course = await _courseRepo.GetByIdAsync(courseId);
                if (course == null)
                    continue;

                decimal linePrice = (course.DiscountPrice != null && course.DiscountPrice > 0)
                    ? course.DiscountPrice.Value
                    : course.Price;

                orderDetails.Add(new OrderDetail
                {
                    CourseId = courseId,
                    Price = linePrice,
                    DiscountApplied = 0
                });
            }

            if (orderDetails.Count == 0)
                throw new InvalidOperationException("Không còn khóa học hợp lệ trong giỏ (đã đăng ký hoặc không tồn tại).");

            decimal total = orderDetails.Sum(d => d.Price - (d.DiscountApplied ?? 0));

            decimal walletUsed = 0;
            if (useWallet && total > 0)
            {
                var wallet = await _walletService.GetOrCreateWalletAsync(userId);
                if (wallet.Balance > 0)
                    walletUsed = Math.Min(wallet.Balance, total);
            }

            var order = new Order
            {
                UserId = userId,
                TotalAmount = total,
                WalletUsed = walletUsed,
                PaymentMethod = "VNPAY",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = orderDetails
            };

            var created = await _orderRepo.CreateOrderAsync(order);

            if (walletUsed > 0)
            {
                await _walletService.DeductFundsAsync(
                    userId,
                    walletUsed,
                    $"Thanh toán giỏ hàng ({orderDetails.Count} khóa)",
                    created.OrderId);
            }

            if (total <= 0)
            {
                created.Status = OrderStatus.Completed;
                created.PaymentMethod = "FREE";
                created.CompletedAt = DateTime.UtcNow;
                await _orderRepo.UpdateOrderAsync(created);
                foreach (var d in created.OrderDetails!)
                    await _enrollmentService.EnrollAsync(userId, d.CourseId, true);
                return created;
            }

            if (walletUsed >= total)
            {
                created.Status = OrderStatus.Completed;
                created.PaymentMethod = "WALLET";
                created.CompletedAt = DateTime.UtcNow;
                await _orderRepo.UpdateOrderAsync(created);
                foreach (var d in created.OrderDetails!)
                    await _enrollmentService.EnrollAsync(userId, d.CourseId, true);
            }

            return created;
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
            if (result && order.OrderDetails != null)
            {
                foreach (var d in order.OrderDetails)
                    await _enrollmentService.EnrollAsync(order.UserId, d.CourseId, true);
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
            // Kiểm tra khóa học đã hoàn thành chưa — không cho phép hoàn tiền nếu đã học xong
            var courseProgress = await _progressService.GetCourseProgressAsync(userId, courseId);
            if (courseProgress != null && courseProgress.IsCompleted)
            {
                return (false, "Bạn đã hoàn thành khóa học này nên không thể yêu cầu hoàn tiền.");
            }

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

        public async Task<Order> CreateTopUpOrderAsync(string userId, decimal amount)
        {
            var order = new Order
            {
                UserId = userId,
                TotalAmount = amount,
                WalletUsed = 0,
                PaymentMethod = "VNPAY_TOPUP",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepo.CreateOrderAsync(order);
            return createdOrder;
        }

        public async Task<bool> CompleteTopUpOrderAsync(int orderId, string transactionId, string gatewayResponse)
        {
            var order = await _orderRepo.GetOrderByOrderIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Completed) return false;

            order.Status = OrderStatus.Completed;
            order.TransactionId = transactionId;
            order.PaymentGatewayResponse = gatewayResponse;
            order.CompletedAt = DateTime.UtcNow;

            var result = await _orderRepo.UpdateOrderAsync(order);

            if (result)
            {
                await _walletService.AddFundsAsync(
                    order.UserId,
                    order.TotalAmount,
                    $"Nạp tiền vào ví",
                    order.OrderId,
                    Models.Entities.WalletTransactionType.TopUp
                );
            }

            return result;
        }
    }
}

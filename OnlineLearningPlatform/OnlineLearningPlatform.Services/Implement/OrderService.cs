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

        public OrderService(IOrderRepository orderRepo, ICourseRepository courseRepo, IEnrollmentService enrollmentService)
        {
            _orderRepo = orderRepo;
            _courseRepo = courseRepo;
            _enrollmentService = enrollmentService;
        }

        public async Task<Order> CreateOrderAsync(string userId, Guid courseId, string paymentMethod)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null) throw new Exception("Course not found");

            decimal finalPrice = (course.DiscountPrice != null && course.DiscountPrice > 0) ? course.DiscountPrice.Value : course.Price;

            var order = new Order
            {
                UserId = userId,
                TotalAmount = finalPrice,
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

            return await _orderRepo.CreateOrderAsync(order);
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

            await _orderRepo.UpdateOrderAsync(order);

            // Tự động enroll sau khi thanh toán thành công
            foreach (var detail in order.OrderDetails)
            {
                await _enrollmentService.EnrollAsync(order.UserId, detail.CourseId);
            }

            return true;
        }

        public async Task<bool> FailOrderAsync(int orderId, string gatewayResponse)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null || order.Status != OrderStatus.Pending) return false;

            order.Status = OrderStatus.Failed;
            order.PaymentGatewayResponse = gatewayResponse;

            return await _orderRepo.UpdateOrderAsync(order);
        }
    }
}

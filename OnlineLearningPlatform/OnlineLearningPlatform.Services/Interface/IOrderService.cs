using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Payment;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, Guid courseId, string paymentMethod, bool useWallet = false, string? couponCode = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetMyOrdersAsync(string userId);
        Task<bool> CompleteOrderAsync(int orderId, string transactionId, string gatewayResponse);
        Task<bool> FailOrderAsync(int orderId, string gatewayResponse);
        Task<(bool Success, string Message)> RefundCourseToWalletAsync(string userId, Guid courseId);
        Task<Order> CreateTopUpOrderAsync(string userId, decimal amount);
        Task<bool> CompleteTopUpOrderAsync(int orderId, string transactionId, string gatewayResponse);
    }
}

using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Payment;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(string userId, Guid courseId, string paymentMethod);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetMyOrdersAsync(string userId);
        Task<bool> CompleteOrderAsync(int orderId, string transactionId, string gatewayResponse);
        Task<bool> FailOrderAsync(int orderId, string gatewayResponse);
    }
}

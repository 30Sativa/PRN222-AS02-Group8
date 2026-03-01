using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order?> GetOrderByTransactionIdAsync(string transactionId);
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
        Task<bool> UpdateOrderAsync(Order order);
    }
}

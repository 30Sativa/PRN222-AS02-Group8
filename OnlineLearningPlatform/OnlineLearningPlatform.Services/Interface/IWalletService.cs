using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IWalletService
    {
        Task<Wallet> GetOrCreateWalletAsync(string userId);
        Task<bool> AddFundsAsync(string userId, decimal amount, string description, int? relatedOrderId = null, WalletTransactionType type = WalletTransactionType.TopUp);
        Task<bool> DeductFundsAsync(string userId, decimal amount, string description, int? relatedOrderId = null);
        Task<List<WalletTransaction>> GetTransactionHistoryAsync(string userId);
    }
}

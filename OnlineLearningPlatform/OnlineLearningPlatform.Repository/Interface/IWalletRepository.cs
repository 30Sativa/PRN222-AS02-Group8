using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByUserIdAsync(string userId);
        Task<Wallet> CreateWalletAsync(Wallet wallet);
        Task<bool> UpdateWalletAsync(Wallet wallet);
        
        Task<WalletTransaction> CreateTransactionAsync(WalletTransaction transaction);
        Task<List<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId);
    }
}

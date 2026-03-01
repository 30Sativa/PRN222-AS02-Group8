using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepo;

        public WalletService(IWalletRepository walletRepo)
        {
            _walletRepo = walletRepo;
        }

        public async Task<Wallet> GetOrCreateWalletAsync(string userId)
        {
            var wallet = await _walletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                wallet = await _walletRepo.CreateWalletAsync(wallet);
            }
            return wallet;
        }

        public async Task<bool> AddFundsAsync(string userId, decimal amount, string description, int? relatedOrderId = null, WalletTransactionType type = WalletTransactionType.TopUp)
        {
            if (amount <= 0) return false;

            var wallet = await GetOrCreateWalletAsync(userId);
            wallet.Balance += amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _walletRepo.UpdateWalletAsync(wallet);

            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount,
                Type = type,
                Description = description,
                OrderId = relatedOrderId,
                CreatedAt = DateTime.UtcNow
            };
            await _walletRepo.CreateTransactionAsync(transaction);

            return true;
        }

        public async Task<bool> DeductFundsAsync(string userId, decimal amount, string description, int? relatedOrderId = null)
        {
            if (amount <= 0) return false;

            var wallet = await GetOrCreateWalletAsync(userId);
            if (wallet.Balance < amount) return false; // Not enough balance

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _walletRepo.UpdateWalletAsync(wallet);

            var transaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amount, // Could be stored as positive or negative based on convention, we use positive and Type=Purchase
                Type = WalletTransactionType.Purchase,
                Description = description,
                OrderId = relatedOrderId,
                CreatedAt = DateTime.UtcNow
            };
            await _walletRepo.CreateTransactionAsync(transaction);

            return true;
        }

        public async Task<List<WalletTransaction>> GetTransactionHistoryAsync(string userId)
        {
            var wallet = await _walletRepo.GetWalletByUserIdAsync(userId);
            if (wallet == null) return new List<WalletTransaction>();

            return await _walletRepo.GetTransactionsByWalletIdAsync(wallet.WalletId);
        }
    }
}

using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task<Coupon?> GetByIdAsync(int couponId);
        Task<List<Coupon>> GetAllAsync(bool includeExpired = false);
        Task<List<Coupon>> GetByCreatorAsync(string teacherId, string? adminId = null);
        Task<Coupon> CreateAsync(Coupon coupon);
        Task<bool> UpdateAsync(Coupon coupon);
        Task<bool> DeleteAsync(int couponId);
        Task<int> GetUsageCountByUserAsync(int couponId, string userId);
        Task<CouponUsage> RecordUsageAsync(CouponUsage usage);
        Task<bool> IncrementUsedCountAsync(int couponId);
    }
}

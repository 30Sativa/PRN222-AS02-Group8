using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ICouponService
    {
        Task<Coupon> CreateCouponAsync(Coupon coupon);
        Task<bool> UpdateCouponAsync(Coupon coupon);
        Task<List<Coupon>> GetAllCouponsAsync(bool includeExpired = false);
        Task<List<Coupon>> GetCouponsByTeacherAsync(string teacherId);
        Task<Coupon?> GetByIdAsync(int couponId);
        Task<bool> DeactivateCouponAsync(int couponId);
        Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateAndCalculateDiscountAsync(
            string code, string userId, Guid courseId, decimal coursePrice);
        Task<Coupon?> GetByCodeAsync(string code);
        Task RecordUsageAsync(int couponId, string userId, int orderId, decimal discountAmount);
    }
}

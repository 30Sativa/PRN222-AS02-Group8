using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _couponRepo;

        public CouponService(ICouponRepository couponRepo)
        {
            _couponRepo = couponRepo;
        }

        public async Task<Coupon> CreateCouponAsync(Coupon coupon)
        {
            return await _couponRepo.CreateAsync(coupon);
        }

        public async Task<bool> UpdateCouponAsync(Coupon coupon)
        {
            return await _couponRepo.UpdateAsync(coupon);
        }

        public async Task<List<Coupon>> GetAllCouponsAsync(bool includeExpired = false)
        {
            return await _couponRepo.GetAllAsync(includeExpired);
        }

        public async Task<List<Coupon>> GetCouponsByTeacherAsync(string teacherId)
        {
            return await _couponRepo.GetByCreatorAsync(teacherId, null);
        }

        public async Task<Coupon?> GetByIdAsync(int couponId)
        {
            return await _couponRepo.GetByIdAsync(couponId);
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _couponRepo.GetByCodeAsync(code);
        }

        public async Task<bool> DeactivateCouponAsync(int couponId)
        {
            return await _couponRepo.DeleteAsync(couponId);
        }

        public async Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateAndCalculateDiscountAsync(
            string code, string userId, Guid courseId, decimal coursePrice)
        {
            var coupon = await _couponRepo.GetByCodeAsync(code);
            if (coupon == null)
                return (false, "Mã khuyến mãi không tồn tại.", 0);

            if (!coupon.IsActive)
                return (false, "Mã khuyến mãi đã bị vô hiệu hóa.", 0);

            var now = DateTime.UtcNow;
            if (now < coupon.StartDate)
                return (false, "Mã khuyến mãi chưa có hiệu lực.", 0);

            if (now > coupon.EndDate)
                return (false, "Mã khuyến mãi đã hết hạn.", 0);

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return (false, "Mã khuyến mãi đã hết lượt sử dụng.", 0);

            if (coupon.MaxUsagePerUser.HasValue)
            {
                var userUsageCount = await _couponRepo.GetUsageCountByUserAsync(coupon.CouponId, userId);
                if (userUsageCount >= coupon.MaxUsagePerUser.Value)
                    return (false, "Bạn đã sử dụng mã khuyến mãi này rồi.", 0);
            }

            if (coupon.IsCourseSpecific && coupon.CourseId != courseId)
                return (false, "Mã khuyến mãi không áp dụng cho khóa học này.", 0);

            if (coupon.MinOrderAmount.HasValue && coursePrice < coupon.MinOrderAmount.Value)
                return (false, $"Đơn hàng tối thiểu {coupon.MinOrderAmount.Value:N0} VNĐ để áp dụng mã này.", 0);

            decimal discount;
            if (coupon.DiscountType == CouponDiscountType.Percentage)
            {
                discount = coursePrice * coupon.DiscountValue / 100m;
                if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                    discount = coupon.MaxDiscountAmount.Value;
            }
            else
            {
                discount = coupon.DiscountValue;
                if (discount > coursePrice) discount = coursePrice;
            }

            return (true, "Áp dụng thành công!", discount);
        }

        public async Task RecordUsageAsync(int couponId, string userId, int orderId, decimal discountAmount)
        {
            var usage = new CouponUsage
            {
                CouponId = couponId,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = DateTime.UtcNow
            };

            await _couponRepo.RecordUsageAsync(usage);
            await _couponRepo.IncrementUsedCountAsync(couponId);
        }
    }
}

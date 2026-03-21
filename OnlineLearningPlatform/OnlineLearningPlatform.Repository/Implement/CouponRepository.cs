using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);
        }

        public async Task<Coupon?> GetByIdAsync(int couponId)
        {
            return await _context.Coupons
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CouponId == couponId);
        }

        public async Task<List<Coupon>> GetAllAsync(bool includeExpired = false)
        {
            var query = _context.Coupons
                .Include(c => c.Course)
                .Include(c => c.CreatedByTeacher)
                .Include(c => c.CreatedByAdmin)
                .AsQueryable();

            if (!includeExpired)
            {
                query = query.Where(c => c.IsActive && c.EndDate >= DateTime.UtcNow);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<Coupon>> GetByCreatorAsync(string teacherId, string? adminId = null)
        {
            return await _context.Coupons
                .Include(c => c.Course)
                .Where(c => c.CreatedByTeacherId == teacherId || c.CreatedByAdminId == adminId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coupon> CreateAsync(Coupon coupon)
        {
            coupon.Code = coupon.Code.ToUpper();
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null) return false;
            coupon.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUsageCountByUserAsync(int couponId, string userId)
        {
            return await _context.CouponUsages
                .CountAsync(cu => cu.CouponId == couponId && cu.UserId == userId);
        }

        public async Task<CouponUsage> RecordUsageAsync(CouponUsage usage)
        {
            _context.CouponUsages.Add(usage);
            await _context.SaveChangesAsync();
            return usage;
        }

        public async Task<bool> IncrementUsedCountAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null) return false;
            coupon.UsedCount++;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

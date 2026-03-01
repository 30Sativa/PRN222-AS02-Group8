using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;

        public CertificateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Certificate?> GetByUserAndCourseAsync(string userId, Guid courseId)
        {
            return await _context.Certificates
                .AsNoTracking()
                .Include(c => c.Course)
                    .ThenInclude(co => co.Teacher)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);
        }

        public async Task<List<Certificate>> GetByUserAsync(string userId)
        {
            return await _context.Certificates
                .AsNoTracking()
                .Include(c => c.Course)
                    .ThenInclude(co => co.Teacher)
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();
        }

        public async Task<Certificate> CreateAsync(Certificate certificate)
        {
            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            return certificate;
        }

        public async Task<bool> ExistsAsync(string userId, Guid courseId)
        {
            return await _context.Certificates
                .AnyAsync(c => c.UserId == userId && c.CourseId == courseId);
        }

        public async Task<List<Certificate>> GetAllAsync()
        {
            return await _context.Certificates
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Teacher)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class SectionRepository : ISectionRepository
    {
        private readonly ApplicationDbContext _context;

        public SectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Section>> GetByCourseAsync(Guid courseId)
        {
            return await _context.Sections
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.OrderIndex)
                .ToListAsync();
        }

        public async Task<Section?> GetByIdAsync(int sectionId)
        {
            return await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }

        public async Task<Section> CreateAsync(Section section)
        {
            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            return section;
        }

        public async Task<bool> UpdateAsync(Section section)
        {
            var existing = await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionId == section.SectionId);

            if (existing == null)
            {
                return false;
            }

            existing.Title = section.Title;
            existing.OrderIndex = section.OrderIndex;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int sectionId)
        {
            var existing = await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (existing == null)
            {
                return false;
            }

            _context.Sections.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSectionInTeacherCourseAsync(int sectionId, string teacherId)
        {
            return await _context.Sections
                .AnyAsync(s => s.SectionId == sectionId && s.Course.TeacherId == teacherId && !s.Course.IsDeleted);
        }

        public async Task<bool> ReorderAsync(Guid courseId, List<int> orderedSectionIds)
        {
            var sections = await _context.Sections
                .Where(s => s.CourseId == courseId)
                .ToListAsync();

            if (!sections.Any())
            {
                return false;
            }

            var allMatch = sections.Select(s => s.SectionId).OrderBy(x => x)
                .SequenceEqual(orderedSectionIds.OrderBy(x => x));

            if (!allMatch)
            {
                return false;
            }

            for (int i = 0; i < orderedSectionIds.Count; i++)
            {
                var section = sections.First(s => s.SectionId == orderedSectionIds[i]);
                section.OrderIndex = i + 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

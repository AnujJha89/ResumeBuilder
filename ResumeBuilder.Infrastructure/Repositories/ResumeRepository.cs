using Microsoft.EntityFrameworkCore;
using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using ResumeBuilder.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeBuilder.Infrastructure.Repositories
{
    public class ResumeRepository : Repository<Resume>, IResumeRepository
    {
        public ResumeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Resume?> GetResumeWithDetailsAsync(int resumeId)
        {
            return await _context.Resumes
                .Include(r => r.Template)
                .Include(r => r.Educations)
                .Include(r => r.Experiences)
                .Include(r => r.Projects)
                .Include(r => r.Skills)
                .Include(r => r.Certifications)
                .Include(r => r.Achievements)
                .FirstOrDefaultAsync(r => r.Id == resumeId);
        }

        public async Task<IEnumerable<Resume>> GetResumesByUserIdAsync(string userId)
        {
            return await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }
    }
}

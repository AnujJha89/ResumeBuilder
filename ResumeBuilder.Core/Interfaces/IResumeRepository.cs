using ResumeBuilder.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResumeBuilder.Core.Interfaces
{
    public interface IResumeRepository : IRepository<Resume>
    {
        Task<Resume?> GetResumeWithDetailsAsync(int resumeId);
        Task<IEnumerable<Resume>> GetResumesByUserIdAsync(string userId);
    }
}

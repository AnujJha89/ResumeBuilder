using ResumeBuilder.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResumeBuilder.Core.Interfaces
{
    public interface IResumeService
    {
        Task<Resume?> GetResumeByIdAsync(int id);
        Task<IEnumerable<Resume>> GetResumesByUserIdAsync(string userId);
        Task CreateResumeAsync(Resume resume);
        Task UpdateResumeAsync(Resume resume);
        Task DeleteResumeAsync(int id);

        // Education CRUD methods
        Task<Education?> GetEducationByIdAsync(int id);
        Task AddEducationAsync(Education education);
        Task UpdateEducationAsync(Education education);
        Task DeleteEducationAsync(int id);

        // Skill CRUD methods
        Task<Skill?> GetSkillByIdAsync(int id);
        Task AddSkillAsync(Skill skill);
        Task UpdateSkillAsync(Skill skill);
        Task DeleteSkillAsync(int id);

        // Project CRUD methods
        Task<Project?> GetProjectByIdAsync(int id);
        Task AddProjectAsync(Project project);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);

        // Experience CRUD methods
        Task<Experience?> GetExperienceByIdAsync(int id);
        Task AddExperienceAsync(Experience experience);
        Task UpdateExperienceAsync(Experience experience);
        Task DeleteExperienceAsync(int id);

        // Certification CRUD methods
        Task<Certification?> GetCertificationByIdAsync(int id);
        Task AddCertificationAsync(Certification certification);
        Task UpdateCertificationAsync(Certification certification);
        Task DeleteCertificationAsync(int id);

        // Achievement CRUD methods
        Task<Achievement?> GetAchievementByIdAsync(int id);
        Task AddAchievementAsync(Achievement achievement);
        Task UpdateAchievementAsync(Achievement achievement);
        Task DeleteAchievementAsync(int id);
    }
}

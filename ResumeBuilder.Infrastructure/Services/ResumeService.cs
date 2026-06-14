using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResumeBuilder.Infrastructure.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IResumeRepository _resumeRepository;
        private readonly IRepository<Education> _educationRepository;
        private readonly IRepository<Skill> _skillRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Experience> _experienceRepository;
        private readonly IRepository<Certification> _certificationRepository;
        private readonly IRepository<Achievement> _achievementRepository;

        public ResumeService(
            IResumeRepository resumeRepository,
            IRepository<Education> educationRepository,
            IRepository<Skill> skillRepository,
            IRepository<Project> projectRepository,
            IRepository<Experience> experienceRepository,
            IRepository<Certification> certificationRepository,
            IRepository<Achievement> achievementRepository)
        {
            _resumeRepository = resumeRepository;
            _educationRepository = educationRepository;
            _skillRepository = skillRepository;
            _projectRepository = projectRepository;
            _experienceRepository = experienceRepository;
            _certificationRepository = certificationRepository;
            _achievementRepository = achievementRepository;
        }

        public async Task<Resume?> GetResumeByIdAsync(int id)
        {
            return await _resumeRepository.GetResumeWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Resume>> GetResumesByUserIdAsync(string userId)
        {
            return await _resumeRepository.GetResumesByUserIdAsync(userId);
        }

        public async Task CreateResumeAsync(Resume resume)
        {
            if (resume == null) throw new ArgumentNullException(nameof(resume));
            
            resume.CreatedAt = DateTime.UtcNow;
            resume.UpdatedAt = DateTime.UtcNow;
            
            await _resumeRepository.AddAsync(resume);
            await _resumeRepository.SaveChangesAsync();
        }

        public async Task UpdateResumeAsync(Resume resume)
        {
            if (resume == null) throw new ArgumentNullException(nameof(resume));
            
            resume.UpdatedAt = DateTime.UtcNow;
            
            _resumeRepository.Update(resume);
            await _resumeRepository.SaveChangesAsync();
        }

        public async Task DeleteResumeAsync(int id)
        {
            var resume = await _resumeRepository.GetByIdAsync(id);
            if (resume != null)
            {
                _resumeRepository.Delete(resume);
                await _resumeRepository.SaveChangesAsync();
            }
        }

        // Education CRUD implementation
        public async Task<Education?> GetEducationByIdAsync(int id)
        {
            return await _educationRepository.GetByIdAsync(id);
        }

        public async Task AddEducationAsync(Education education)
        {
            if (education == null) throw new ArgumentNullException(nameof(education));
            await _educationRepository.AddAsync(education);
            await _educationRepository.SaveChangesAsync();
        }

        public async Task UpdateEducationAsync(Education education)
        {
            if (education == null) throw new ArgumentNullException(nameof(education));
            _educationRepository.Update(education);
            await _educationRepository.SaveChangesAsync();
        }

        public async Task DeleteEducationAsync(int id)
        {
            var education = await _educationRepository.GetByIdAsync(id);
            if (education != null)
            {
                _educationRepository.Delete(education);
                await _educationRepository.SaveChangesAsync();
            }
        }

        // Skill CRUD implementation
        public async Task<Skill?> GetSkillByIdAsync(int id)
        {
            return await _skillRepository.GetByIdAsync(id);
        }

        public async Task AddSkillAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            await _skillRepository.AddAsync(skill);
            await _skillRepository.SaveChangesAsync();
        }

        public async Task UpdateSkillAsync(Skill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            _skillRepository.Update(skill);
            await _skillRepository.SaveChangesAsync();
        }

        public async Task DeleteSkillAsync(int id)
        {
            var skill = await _skillRepository.GetByIdAsync(id);
            if (skill != null)
            {
                _skillRepository.Delete(skill);
                await _skillRepository.SaveChangesAsync();
            }
        }

        // Project CRUD implementation
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _projectRepository.GetByIdAsync(id);
        }

        public async Task AddProjectAsync(Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            _projectRepository.Update(project);
            await _projectRepository.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project != null)
            {
                _projectRepository.Delete(project);
                await _projectRepository.SaveChangesAsync();
            }
        }

        // Experience CRUD implementation
        public async Task<Experience?> GetExperienceByIdAsync(int id)
        {
            return await _experienceRepository.GetByIdAsync(id);
        }

        public async Task AddExperienceAsync(Experience experience)
        {
            if (experience == null) throw new ArgumentNullException(nameof(experience));
            await _experienceRepository.AddAsync(experience);
            await _experienceRepository.SaveChangesAsync();
        }

        public async Task UpdateExperienceAsync(Experience experience)
        {
            if (experience == null) throw new ArgumentNullException(nameof(experience));
            _experienceRepository.Update(experience);
            await _experienceRepository.SaveChangesAsync();
        }

        public async Task DeleteExperienceAsync(int id)
        {
            var experience = await _experienceRepository.GetByIdAsync(id);
            if (experience != null)
            {
                _experienceRepository.Delete(experience);
                await _experienceRepository.SaveChangesAsync();
            }
        }

        // Certification CRUD implementation
        public async Task<Certification?> GetCertificationByIdAsync(int id)
        {
            return await _certificationRepository.GetByIdAsync(id);
        }

        public async Task AddCertificationAsync(Certification certification)
        {
            if (certification == null) throw new ArgumentNullException(nameof(certification));
            await _certificationRepository.AddAsync(certification);
            await _certificationRepository.SaveChangesAsync();
        }

        public async Task UpdateCertificationAsync(Certification certification)
        {
            if (certification == null) throw new ArgumentNullException(nameof(certification));
            _certificationRepository.Update(certification);
            await _certificationRepository.SaveChangesAsync();
        }

        public async Task DeleteCertificationAsync(int id)
        {
            var certification = await _certificationRepository.GetByIdAsync(id);
            if (certification != null)
            {
                _certificationRepository.Delete(certification);
                await _certificationRepository.SaveChangesAsync();
            }
        }

        // Achievement CRUD implementation
        public async Task<Achievement?> GetAchievementByIdAsync(int id)
        {
            return await _achievementRepository.GetByIdAsync(id);
        }

        public async Task AddAchievementAsync(Achievement achievement)
        {
            if (achievement == null) throw new ArgumentNullException(nameof(achievement));
            await _achievementRepository.AddAsync(achievement);
            await _achievementRepository.SaveChangesAsync();
        }

        public async Task UpdateAchievementAsync(Achievement achievement)
        {
            if (achievement == null) throw new ArgumentNullException(nameof(achievement));
            _achievementRepository.Update(achievement);
            await _achievementRepository.SaveChangesAsync();
        }

        public async Task DeleteAchievementAsync(int id)
        {
            var achievement = await _achievementRepository.GetByIdAsync(id);
            if (achievement != null)
            {
                _achievementRepository.Delete(achievement);
                await _achievementRepository.SaveChangesAsync();
            }
        }
    }
}

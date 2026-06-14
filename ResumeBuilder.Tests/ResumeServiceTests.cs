using Moq;
using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using ResumeBuilder.Infrastructure.Services;
using Xunit;

namespace ResumeBuilder.Tests
{
    /// <summary>
    /// Unit tests for ResumeService.
    ///
    /// BEGINNER GUIDE TO UNIT TESTING:
    /// ─────────────────────────────────
    /// Unit testing means testing one small piece of code in isolation.
    /// We DON'T connect to a real database. Instead, we use "Mocks".
    ///
    /// WHAT IS A MOCK?
    ///   A Mock is a fake object that pretends to be a real repository.
    ///   You define what it returns when methods are called (Arrange),
    ///   run the code under test (Act),
    ///   then verify the results (Assert).
    ///   This is called the AAA pattern: Arrange → Act → Assert.
    ///
    /// WHY MOCK?
    ///   - Tests run instantly (no DB connection needed)
    ///   - Tests are predictable (no external state)
    ///   - Tests are isolated (only test one thing at a time)
    /// </summary>
    public class ResumeServiceTests
    {
        // ── Shared mock objects (created fresh for each test) ──────────────────
        private readonly Mock<IResumeRepository> _mockResumeRepo;
        private readonly Mock<IRepository<Education>> _mockEducationRepo;
        private readonly Mock<IRepository<Skill>> _mockSkillRepo;
        private readonly Mock<IRepository<Project>> _mockProjectRepo;
        private readonly Mock<IRepository<Experience>> _mockExperienceRepo;
        private readonly Mock<IRepository<Certification>> _mockCertificationRepo;
        private readonly Mock<IRepository<Achievement>> _mockAchievementRepo;
        private readonly ResumeService _service;

        public ResumeServiceTests()
        {
            // Create fake (mock) implementations of all repository interfaces
            _mockResumeRepo = new Mock<IResumeRepository>();
            _mockEducationRepo = new Mock<IRepository<Education>>();
            _mockSkillRepo = new Mock<IRepository<Skill>>();
            _mockProjectRepo = new Mock<IRepository<Project>>();
            _mockExperienceRepo = new Mock<IRepository<Experience>>();
            _mockCertificationRepo = new Mock<IRepository<Certification>>();
            _mockAchievementRepo = new Mock<IRepository<Achievement>>();

            // Create the real service, injecting all the fake repositories
            _service = new ResumeService(
                _mockResumeRepo.Object,
                _mockEducationRepo.Object,
                _mockSkillRepo.Object,
                _mockProjectRepo.Object,
                _mockExperienceRepo.Object,
                _mockCertificationRepo.Object,
                _mockAchievementRepo.Object
            );
        }

        // ══════════════════════════════════════════════════════════════════
        //  Resume CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Resume")]
        public async Task CreateResumeAsync_ShouldSetTimestamps_AndCallAddAndSave()
        {
            // Arrange: create a minimal resume object
            var resume = new Resume
            {
                Title = "Test Resume",
                UserId = "user-123"
            };

            // Act: call the method we're testing
            await _service.CreateResumeAsync(resume);

            // Assert:
            // 1. CreatedAt should have been set (not left as default DateTime)
            Assert.NotEqual(default, resume.CreatedAt);

            // 2. UpdatedAt should have been set
            Assert.NotEqual(default, resume.UpdatedAt);

            // 3. Both timestamps should be roughly "now" (within 5 seconds)
            Assert.True((DateTime.UtcNow - resume.CreatedAt).TotalSeconds < 5);

            // 4. Verify AddAsync was called exactly once with our resume
            _mockResumeRepo.Verify(r => r.AddAsync(resume), Times.Once);

            // 5. Verify SaveChangesAsync was called exactly once
            _mockResumeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Resume")]
        public async Task CreateResumeAsync_ShouldThrowArgumentNullException_WhenResumeIsNull()
        {
            // Arrange: null resume
            Resume? nullResume = null;

            // Act + Assert: verify that passing null throws the right exception
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateResumeAsync(nullResume!)
            );

            // Also verify that AddAsync was NEVER called (we threw before reaching it)
            _mockResumeRepo.Verify(r => r.AddAsync(It.IsAny<Resume>()), Times.Never);
        }

        [Fact]
        [Trait("Category", "Resume")]
        public async Task UpdateResumeAsync_ShouldUpdateTimestamp_AndCallUpdateAndSave()
        {
            // Arrange
            var originalTime = new DateTime(2020, 1, 1);
            var resume = new Resume
            {
                Id = 1,
                Title = "My Resume",
                UpdatedAt = originalTime
            };

            // Act
            await _service.UpdateResumeAsync(resume);

            // Assert: UpdatedAt should be more recent than the original
            Assert.True(resume.UpdatedAt > originalTime);
            _mockResumeRepo.Verify(r => r.Update(resume), Times.Once);
            _mockResumeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Resume")]
        public async Task DeleteResumeAsync_ShouldDeleteAndSave_WhenResumeExists()
        {
            // Arrange: configure the mock to return a resume when GetByIdAsync(1) is called
            var resume = new Resume { Id = 1, Title = "To Delete" };
            _mockResumeRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(resume);

            // Act
            await _service.DeleteResumeAsync(1);

            // Assert: Delete + SaveChangesAsync must have been called
            _mockResumeRepo.Verify(r => r.Delete(resume), Times.Once);
            _mockResumeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Resume")]
        public async Task DeleteResumeAsync_ShouldDoNothing_WhenResumeNotFound()
        {
            // Arrange: configure mock to return null (resume doesn't exist)
            _mockResumeRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Resume?)null);

            // Act: call delete on a non-existent ID
            await _service.DeleteResumeAsync(999);

            // Assert: Delete should never have been called
            _mockResumeRepo.Verify(r => r.Delete(It.IsAny<Resume>()), Times.Never);
            _mockResumeRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Education CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Education")]
        public async Task AddEducationAsync_ShouldCallAddAndSave()
        {
            // Arrange
            var education = new Education
            {
                ResumeId = 1,
                Degree = "B.Tech Computer Science",
                InstitutionName = "Delhi Technological University",
                PassingYear = 2028,
                PercentageOrCGPA = "9.2 CGPA"
            };

            // Act
            await _service.AddEducationAsync(education);

            // Assert
            _mockEducationRepo.Verify(r => r.AddAsync(education), Times.Once);
            _mockEducationRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Education")]
        public async Task AddEducationAsync_ShouldThrow_WhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.AddEducationAsync(null!)
            );
        }

        // ══════════════════════════════════════════════════════════════════
        //  Skill CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Skill")]
        public async Task AddSkillAsync_ShouldCallAddAndSave()
        {
            var skill = new Skill
            {
                ResumeId = 1,
                SkillName = "C#",
                SkillCategory = "Languages",
                Proficiency = "Expert"
            };

            await _service.AddSkillAsync(skill);

            _mockSkillRepo.Verify(r => r.AddAsync(skill), Times.Once);
            _mockSkillRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Skill")]
        public async Task DeleteSkillAsync_ShouldDelete_WhenExists()
        {
            var skill = new Skill { Id = 5, SkillName = "Python" };
            _mockSkillRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(skill);

            await _service.DeleteSkillAsync(5);

            _mockSkillRepo.Verify(r => r.Delete(skill), Times.Once);
            _mockSkillRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Project CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Project")]
        public async Task AddProjectAsync_ShouldCallAddAndSave()
        {
            var project = new Project
            {
                ResumeId = 1,
                ProjectTitle = "Resume Builder",
                TechnologiesUsed = "ASP.NET Core, EF Core, Bootstrap 5",
                Description = "A multi-step wizard resume builder"
            };

            await _service.AddProjectAsync(project);

            _mockProjectRepo.Verify(r => r.AddAsync(project), Times.Once);
            _mockProjectRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Experience CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Experience")]
        public async Task AddExperienceAsync_ShouldCallAddAndSave()
        {
            var experience = new Experience
            {
                ResumeId = 1,
                CompanyName = "JetBrains",
                Position = "Software Engineer Intern",
                StartDate = new DateTime(2025, 5, 1),
                IsCurrent = false,
                EndDate = new DateTime(2025, 8, 31)
            };

            await _service.AddExperienceAsync(experience);

            _mockExperienceRepo.Verify(r => r.AddAsync(experience), Times.Once);
            _mockExperienceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Certification CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Certification")]
        public async Task AddCertificationAsync_ShouldCallAddAndSave()
        {
            var cert = new Certification
            {
                ResumeId = 1,
                CertificateName = "AZ-204: Azure Developer Associate",
                IssuingOrganization = "Microsoft",
                IssueDate = new DateTime(2025, 11, 1)
            };

            await _service.AddCertificationAsync(cert);

            _mockCertificationRepo.Verify(r => r.AddAsync(cert), Times.Once);
            _mockCertificationRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Achievement CRUD Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Achievement")]
        public async Task AddAchievementAsync_ShouldCallAddAndSave()
        {
            var achievement = new Achievement
            {
                ResumeId = 1,
                Title = "1st Place - Smart India Hackathon 2025",
                Description = "Won first place among 500+ teams nationwide.",
                Category = "Awards"
            };

            await _service.AddAchievementAsync(achievement);

            _mockAchievementRepo.Verify(r => r.AddAsync(achievement), Times.Once);
            _mockAchievementRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        [Trait("Category", "Achievement")]
        public async Task AddAchievementAsync_ShouldThrow_WhenNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.AddAchievementAsync(null!)
            );
        }
    }
}

using ResumeBuilder.Core.Entities;
using ResumeBuilder.Infrastructure.Services;
using Xunit;

namespace ResumeBuilder.Tests
{
    /// <summary>
    /// Unit tests for the HTML resume builder logic inside PdfService.
    ///
    /// WHAT ARE WE TESTING HERE?
    /// The PdfService.BuildResumeHtml() method takes a Resume object and
    /// produces an HTML string. We can test this in isolation without
    /// launching any browser — we just check the string output.
    ///
    /// These tests verify:
    /// 1. Resume data appears in the HTML (name, job title, sections)
    /// 2. Special characters are HTML-encoded (security)
    /// 3. Empty sections are skipped gracefully
    /// </summary>
    public class PdfServiceHtmlBuilderTests
    {
        /// <summary>
        /// Helper: creates a fully populated Resume for testing.
        /// This is a factory method — a clean way to avoid repeating setup code.
        /// </summary>
        private static Resume CreateSampleResume() => new Resume
        {
            Id = 42,
            FullName = "Anuj Kumar Jha",
            JobTitle = "Software Engineer Intern",
            Email = "anuj@example.com",
            Phone = "+91-9999999999",
            Address = "New Delhi, India",
            GitHubUrl = "https://github.com/anujkjha",
            LinkedInUrl = "https://linkedin.com/in/anujkjha",
            ProfessionalSummary = "Second-year CS student passionate about clean architecture.",
            Educations = new List<Education>
            {
                new Education
                {
                    Degree = "B.Tech Computer Science",
                    InstitutionName = "Delhi Technological University",
                    BoardOrUniversity = "DTU",
                    PassingYear = 2028,
                    PercentageOrCGPA = "9.2 CGPA",
                    DisplayOrder = 1
                }
            },
            Skills = new List<Skill>
            {
                new Skill
                {
                    SkillName = "C#",
                    SkillCategory = "Languages",
                    Proficiency = "Expert",
                    DisplayOrder = 1
                },
                new Skill
                {
                    SkillName = "ASP.NET Core",
                    SkillCategory = "Frameworks",
                    Proficiency = "Advanced",
                    DisplayOrder = 1
                }
            },
            Projects = new List<Project>
            {
                new Project
                {
                    ProjectTitle = "Resume Builder",
                    Description = "A 10-step wizard resume builder.",
                    TechnologiesUsed = "ASP.NET Core, EF Core",
                    Duration = "June 2026",
                    DisplayOrder = 1
                }
            },
            Experiences = new List<Experience>
            {
                new Experience
                {
                    Position = "Software Engineer Intern",
                    CompanyName = "JetBrains",
                    StartDate = new DateTime(2025, 5, 1),
                    EndDate = new DateTime(2025, 8, 31),
                    IsCurrent = false,
                    Responsibilities = "Built wizard-style UI components.",
                    DisplayOrder = 1
                }
            },
            Certifications = new List<Certification>
            {
                new Certification
                {
                    CertificateName = "AZ-204: Azure Developer",
                    IssuingOrganization = "Microsoft",
                    IssueDate = new DateTime(2025, 11, 1),
                    DisplayOrder = 1
                }
            },
            Achievements = new List<Achievement>
            {
                new Achievement
                {
                    Title = "1st Place - Smart India Hackathon",
                    Description = "Won among 500+ teams.",
                    Category = "Hackathons",
                    DisplayOrder = 1
                }
            }
        };

        // ══════════════════════════════════════════════════════════════════
        //  Header / Personal Info Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainFullName()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("Anuj Kumar Jha", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainJobTitle()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("Software Engineer Intern", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainEmail()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("anuj@example.com", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainProfessionalSummary()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("Second-year CS student", html);
            Assert.Contains("Professional Summary", html);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Section Presence Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainEducationSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("B.Tech Computer Science", html);
            Assert.Contains("Delhi Technological University", html);
            Assert.Contains("Education", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainSkillsSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("C#", html);
            Assert.Contains("Languages", html);
            Assert.Contains("ASP.NET Core", html);
            Assert.Contains("Technical Skills", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainProjectsSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("Resume Builder", html);
            Assert.Contains("A 10-step wizard resume builder", html);
            Assert.Contains("Projects", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainExperienceSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("JetBrains", html);
            Assert.Contains("Built wizard-style UI components", html);
            Assert.Contains("Experience", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainCertificationsSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("AZ-204: Azure Developer", html);
            Assert.Contains("Microsoft", html);
            Assert.Contains("Certifications", html);
        }

        [Fact]
        [Trait("Category", "HTML")]
        public void BuildResumeHtml_ShouldContainAchievementsSection()
        {
            var resume = CreateSampleResume();
            var html = PdfService.BuildResumeHtml(resume);
            Assert.Contains("Smart India Hackathon", html);
            Assert.Contains("Won among 500+ teams", html);
            Assert.Contains("Achievements", html);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Security: HTML Encoding Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "Security")]
        public void HtmlEncode_ShouldEncodeAmpersand()
        {
            // & must become &amp; to prevent malformed HTML
            var result = PdfService.HtmlEncode("C & Python");
            Assert.Equal("C &amp; Python", result);
        }

        [Fact]
        [Trait("Category", "Security")]
        public void HtmlEncode_ShouldEncodeLessThanSign()
        {
            // < must become &lt; to prevent HTML injection
            var result = PdfService.HtmlEncode("<script>alert('xss')</script>");
            Assert.Contains("&lt;script&gt;", result);
            Assert.DoesNotContain("<script>", result);
        }

        [Fact]
        [Trait("Category", "Security")]
        public void HtmlEncode_ShouldHandleNullInput()
        {
            // Null should return empty string, not throw
            var result = PdfService.HtmlEncode(null!);
            Assert.Equal("", result);
        }

        [Fact]
        [Trait("Category", "Security")]
        public void BuildResumeHtml_ShouldEncodeXssInName()
        {
            // Test that malicious input in the resume name is safely encoded
            var resume = new Resume
            {
                FullName = "<script>alert('hack')</script>",
                JobTitle = "Developer",
                Educations = new List<Education>(),
                Skills = new List<Skill>(),
                Projects = new List<Project>(),
                Experiences = new List<Experience>(),
                Certifications = new List<Certification>(),
                Achievements = new List<Achievement>()
            };

            var html = PdfService.BuildResumeHtml(resume);

            // The raw <script> tag must NOT appear in the output
            Assert.DoesNotContain("<script>alert('hack')</script>", html);
            // The encoded version must appear instead
            Assert.Contains("&lt;script&gt;", html);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Empty Data Graceful Handling Tests
        // ══════════════════════════════════════════════════════════════════

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void BuildResumeHtml_ShouldNotContainSkillsSection_WhenSkillsEmpty()
        {
            var resume = CreateSampleResume();
            resume.Skills = new List<Skill>(); // clear skills

            var html = PdfService.BuildResumeHtml(resume);

            // The "Technical Skills" section header should not appear
            Assert.DoesNotContain("Technical Skills", html);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void BuildResumeHtml_ShouldNotContainSummary_WhenSummaryIsEmpty()
        {
            var resume = CreateSampleResume();
            resume.ProfessionalSummary = null; // no summary

            var html = PdfService.BuildResumeHtml(resume);

            // The section-title content for summary should be absent.
            // (CSS class names can still appear — we check the visible heading text)
            Assert.DoesNotContain("section-title\">Professional Summary", html);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void BuildResumeHtml_ShouldNotContainAchievements_WhenEmpty()
        {
            var resume = CreateSampleResume();
            resume.Achievements = new List<Achievement>();

            var html = PdfService.BuildResumeHtml(resume);

            // CSS has ".achievement-item" class names but the visible section heading
            // should not appear when there are no achievements.
            Assert.DoesNotContain("section-title\">Achievements", html);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        public void BuildResumeHtml_ShouldProduceValidHtml_WithMinimalResume()
        {
            // A resume with only a name should still produce a valid HTML document
            var resume = new Resume
            {
                FullName = "Minimal User",
                JobTitle = "",
                Educations = new List<Education>(),
                Skills = new List<Skill>(),
                Projects = new List<Project>(),
                Experiences = new List<Experience>(),
                Certifications = new List<Certification>(),
                Achievements = new List<Achievement>()
            };

            var html = PdfService.BuildResumeHtml(resume);

            // Should still be valid HTML with DOCTYPE and body tags
            Assert.Contains("<!DOCTYPE html>", html);
            Assert.Contains("<html", html);
            Assert.Contains("</html>", html);
            Assert.Contains("Minimal User", html);
        }
    }
}

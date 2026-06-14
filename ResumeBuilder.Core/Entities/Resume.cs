using System;
using System.Collections.Generic;

namespace ResumeBuilder.Core.Entities
{
    public class Resume
    {
        public int Id { get; set; }
        
        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int TemplateId { get; set; }

        // Resume Details
        public string Title { get; set; } = string.Empty; // e.g., "Backend Resume"
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? GitHubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string JobTitle { get; set; } = string.Empty; // e.g., "Software Engineer"
        public string ProfessionalSummary { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ResumeTemplate? Template { get; set; }

        public virtual ICollection<Education> Educations { get; set; } = new List<Education>();
        public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
        public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
        public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
}

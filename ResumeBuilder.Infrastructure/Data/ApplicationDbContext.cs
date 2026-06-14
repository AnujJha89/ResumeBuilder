using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeBuilder.Core.Entities;

namespace ResumeBuilder.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Resume> Resumes { get; set; } = null!;
        public DbSet<ResumeTemplate> ResumeTemplates { get; set; } = null!;
        public DbSet<Education> Educations { get; set; } = null!;
        public DbSet<Experience> Experiences { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<Certification> Certifications { get; set; } = null!;
        public DbSet<Achievement> Achievements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Call base method first to configure Identity schemas
            base.OnModelCreating(builder);

            // Configure Resume relationships
            builder.Entity<Resume>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).HasMaxLength(100).IsRequired();
                entity.Property(r => r.FullName).HasMaxLength(100).IsRequired();
                entity.Property(r => r.Email).HasMaxLength(256).IsRequired();
                entity.Property(r => r.Phone).HasMaxLength(20).IsRequired();
                entity.Property(r => r.Address).HasMaxLength(500).IsRequired();
                entity.Property(r => r.GitHubUrl).HasMaxLength(256);
                entity.Property(r => r.LinkedInUrl).HasMaxLength(256);
                entity.Property(r => r.ProfilePicturePath).HasMaxLength(500);
                entity.Property(r => r.JobTitle).HasMaxLength(100).IsRequired();
                entity.Property(r => r.ProfessionalSummary).HasMaxLength(4000).IsRequired();

                // Relationship: User -> Resumes (1-to-many)
                entity.HasOne(r => r.User)
                      .WithMany(u => u.Resumes)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Template -> Resumes (1-to-many)
                entity.HasOne(r => r.Template)
                      .WithMany(t => t.Resumes)
                      .HasForeignKey(r => r.TemplateId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ResumeTemplate
            builder.Entity<ResumeTemplate>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
                entity.Property(t => t.LaTeXCodeTemplate).IsRequired();
                entity.Property(t => t.ThumbnailPath).HasMaxLength(500).IsRequired();
                entity.Property(t => t.IsActive).HasDefaultValue(true);
            });

            // Configure Education (Child of Resume)
            builder.Entity<Education>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Degree).HasMaxLength(100).IsRequired();
                entity.Property(e => e.InstitutionName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.BoardOrUniversity).HasMaxLength(150).IsRequired();
                entity.Property(e => e.PercentageOrCGPA).HasMaxLength(20).IsRequired();

                // Relationship: Resume -> Educations (1-to-many)
                entity.HasOne(e => e.Resume)
                      .WithMany(r => r.Educations)
                      .HasForeignKey(e => e.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Experience (Child of Resume)
            builder.Entity<Experience>(entity =>
            {
                entity.HasKey(ex => ex.Id);
                entity.Property(ex => ex.CompanyName).HasMaxLength(150).IsRequired();
                entity.Property(ex => ex.Position).HasMaxLength(100).IsRequired();
                entity.Property(ex => ex.Responsibilities).IsRequired();

                // Relationship: Resume -> Experiences (1-to-many)
                entity.HasOne(ex => ex.Resume)
                      .WithMany(r => r.Experiences)
                      .HasForeignKey(ex => ex.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Project (Child of Resume)
            builder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ProjectTitle).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Description).IsRequired();
                entity.Property(p => p.TechnologiesUsed).HasMaxLength(300).IsRequired();
                entity.Property(p => p.GitHubLink).HasMaxLength(256);
                entity.Property(p => p.Duration).HasMaxLength(100).IsRequired();

                // Relationship: Resume -> Projects (1-to-many)
                entity.HasOne(p => p.Resume)
                      .WithMany(r => r.Projects)
                      .HasForeignKey(p => p.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Skill (Child of Resume)
            builder.Entity<Skill>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.SkillName).HasMaxLength(100).IsRequired();
                entity.Property(s => s.SkillCategory).HasMaxLength(100).IsRequired();
                entity.Property(s => s.Proficiency).HasMaxLength(50);

                // Relationship: Resume -> Skills (1-to-many)
                entity.HasOne(s => s.Resume)
                      .WithMany(r => r.Skills)
                      .HasForeignKey(s => s.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Certification (Child of Resume)
            builder.Entity<Certification>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CertificateName).HasMaxLength(150).IsRequired();
                entity.Property(c => c.IssuingOrganization).HasMaxLength(150).IsRequired();
                entity.Property(c => c.Description).IsRequired();
                entity.Property(c => c.CredentialUrl).HasMaxLength(256);

                // Relationship: Resume -> Certifications (1-to-many)
                entity.HasOne(c => c.Resume)
                      .WithMany(r => r.Certifications)
                      .HasForeignKey(c => c.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Achievement (Child of Resume)
            builder.Entity<Achievement>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
                entity.Property(a => a.Description).HasMaxLength(1000);
                entity.Property(a => a.Category).HasMaxLength(100).IsRequired().HasDefaultValue("Awards");

                // Relationship: Resume -> Achievements (1-to-many)
                entity.HasOne(a => a.Resume)
                      .WithMany(r => r.Achievements)
                      .HasForeignKey(a => a.ResumeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

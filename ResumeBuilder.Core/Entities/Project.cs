namespace ResumeBuilder.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }
        
        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string ProjectTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechnologiesUsed { get; set; } = string.Empty; // Store as comma-separated values (e.g. "ASP.NET Core, React")
        public string? GitHubLink { get; set; }
        public string Duration { get; set; } = string.Empty; // e.g., "Jan 2026 - Mar 2026"
        
        // Sorting position
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

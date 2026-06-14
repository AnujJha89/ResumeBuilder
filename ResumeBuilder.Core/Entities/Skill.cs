namespace ResumeBuilder.Core.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        
        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string SkillName { get; set; } = string.Empty; // e.g. "C#", "SQL Server"
        public string SkillCategory { get; set; } = string.Empty; // e.g. "Programming Languages", "Databases"
        public string? Proficiency { get; set; } // e.g. "Expert", "Intermediate"
        
        // Sorting position
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

namespace ResumeBuilder.Core.Entities
{
    public class Achievement
    {
        public int Id { get; set; }
        
        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string Title { get; set; } = string.Empty; // e.g. "1st Place in Smart India Hackathon"
        public string? Description { get; set; } // Description of what was achieved
        public string Category { get; set; } = string.Empty; // e.g. Awards, Hackathons, etc.

        // Sorting position
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

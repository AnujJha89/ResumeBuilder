using System;

namespace ResumeBuilder.Core.Entities
{
    public class Experience
    {
        public int Id { get; set; }
        
        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string Responsibilities { get; set; } = string.Empty; // Store bullet points separated by newlines
        
        // Sorting position
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

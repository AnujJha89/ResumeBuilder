namespace ResumeBuilder.Core.Entities
{
    public class Education
    {
        public int Id { get; set; }
        
        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string Degree { get; set; } = string.Empty; // e.g., B.Tech, Matriculation
        public string InstitutionName { get; set; } = string.Empty;
        public string BoardOrUniversity { get; set; } = string.Empty;
        public int PassingYear { get; set; }
        public string PercentageOrCGPA { get; set; } = string.Empty;
        
        // Sorting position of this education entry
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

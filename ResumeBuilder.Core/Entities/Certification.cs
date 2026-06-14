using System;

namespace ResumeBuilder.Core.Entities
{
    public class Certification
    {
        public int Id { get; set; }

        // Foreign Key to Resume
        public int ResumeId { get; set; }

        public string CertificateName { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? CredentialUrl { get; set; }
        
        // Sorting position
        public int DisplayOrder { get; set; }

        // Navigation property
        public virtual Resume? Resume { get; set; }
    }
}

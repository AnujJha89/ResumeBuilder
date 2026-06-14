using System.Collections.Generic;

namespace ResumeBuilder.Core.Entities
{
    public class ResumeTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LaTeXCodeTemplate { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    }
}

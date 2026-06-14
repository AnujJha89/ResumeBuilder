using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ResumeBuilder.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePhotoPath { get; set; }

        // Navigation property
        public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    }
}

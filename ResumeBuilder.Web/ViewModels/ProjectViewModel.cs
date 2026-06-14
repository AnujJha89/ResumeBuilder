using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Project Title is required.")]
        [Display(Name = "Project Title")]
        [StringLength(150)]
        public string ProjectTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Project Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Technologies Used are required.")]
        [Display(Name = "Technologies Used (e.g. ASP.NET Core, React, SQL Server)")]
        [StringLength(300)]
        public string TechnologiesUsed { get; set; } = string.Empty;

        [Display(Name = "GitHub Link (Optional)")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(256)]
        public string? GitHubLink { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Display(Name = "Duration (e.g. Jan 2026 - Mar 2026, 3 Months)")]
        [StringLength(100)]
        public string Duration { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}

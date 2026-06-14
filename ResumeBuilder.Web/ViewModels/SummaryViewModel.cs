using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class SummaryViewModel
    {
        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Job title is required.")]
        [Display(Name = "Target Job Title (e.g. Backend Developer)")]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Professional summary is required.")]
        [Display(Name = "Professional Summary")]
        [StringLength(4000)]
        public string ProfessionalSummary { get; set; } = string.Empty;
    }
}

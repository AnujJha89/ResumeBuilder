using System;
using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class ExperienceViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        [Display(Name = "Company Name")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Position is required.")]
        [Display(Name = "Position / Role (e.g. Frontend Intern)")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "I currently work here")]
        public bool IsCurrent { get; set; }

        [Required(ErrorMessage = "Responsibilities description is required.")]
        [Display(Name = "Responsibilities & Accomplishments (bullet points suggested)")]
        public string Responsibilities { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}

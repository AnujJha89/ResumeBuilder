using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class EducationViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Degree / Certificate title is required.")]
        [Display(Name = "Degree or Certificate (e.g. B.Tech, Class XII)")]
        [StringLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required(ErrorMessage = "Institution name is required.")]
        [Display(Name = "School / College / Institution Name")]
        [StringLength(150)]
        public string InstitutionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Board or University name is required.")]
        [Display(Name = "Board or University")]
        [StringLength(150)]
        public string BoardOrUniversity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passing year is required.")]
        [Display(Name = "Passing Year")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
        public int PassingYear { get; set; }

        [Required(ErrorMessage = "Percentage or CGPA is required.")]
        [Display(Name = "Percentage or CGPA (e.g. 9.2 CGPA, 88%)")]
        [StringLength(20)]
        public string PercentageOrCGPA { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}

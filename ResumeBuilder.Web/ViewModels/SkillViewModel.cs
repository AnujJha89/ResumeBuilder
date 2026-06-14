using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class SkillViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Skill Name is required.")]
        [Display(Name = "Skill Name (e.g. C#, React)")]
        [StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Skill Category is required.")]
        [Display(Name = "Category")]
        [StringLength(100)]
        public string SkillCategory { get; set; } = string.Empty; // e.g. "Programming Languages", "Frameworks & Libraries", "Databases", "Tools"

        [Display(Name = "Proficiency (Optional)")]
        [StringLength(50)]
        public string? Proficiency { get; set; } // e.g. "Beginner", "Intermediate", "Advanced", "Expert"

        public int DisplayOrder { get; set; }
    }
}

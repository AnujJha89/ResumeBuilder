using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class AchievementViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Achievement Title is required.")]
        [Display(Name = "Achievement or Honor (e.g. 1st Rank in Hackathon)")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description (Optional details)")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category (e.g. Awards, Hackathons)")]
        [StringLength(100)]
        public string Category { get; set; } = "Awards";

        public int DisplayOrder { get; set; }
    }
}

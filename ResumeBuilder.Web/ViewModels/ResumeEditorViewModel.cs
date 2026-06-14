using System.ComponentModel.DataAnnotations;
using ResumeBuilder.Core.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ResumeBuilder.Web.ViewModels
{
    public class ResumeEditorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Resume title is required.")]
        [Display(Name = "Resume Title (e.g. Software Dev Resume)")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a resume template.")]
        [Display(Name = "Template")]
        public int TemplateId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "GitHub Profile URL")]
        [Url(ErrorMessage = "Invalid URL.")]
        [StringLength(256)]
        public string? GitHubUrl { get; set; }

        [Display(Name = "LinkedIn Profile URL")]
        [Url(ErrorMessage = "Invalid URL.")]
        [StringLength(256)]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "Target Job Title (e.g. Full Stack Developer)")]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Display(Name = "Professional Summary")]
        [DataType(DataType.MultilineText)]
        [StringLength(4000)]
        public string ProfessionalSummary { get; set; } = string.Empty;

        public string? ProfilePicturePath { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        // View support properties
        public IEnumerable<ResumeTemplate>? AvailableTemplates { get; set; }
    }
}

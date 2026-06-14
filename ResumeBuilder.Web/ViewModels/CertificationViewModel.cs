using System;
using System.ComponentModel.DataAnnotations;

namespace ResumeBuilder.Web.ViewModels
{
    public class CertificationViewModel
    {
        public int Id { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required(ErrorMessage = "Certification Name is required.")]
        [Display(Name = "Certification / Award Name")]
        [StringLength(150)]
        public string CertificateName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issuing Organization is required.")]
        [Display(Name = "Issuing Organization (e.g. Google, Coursera)")]
        [StringLength(150)]
        public string IssuingOrganization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Issue Date is required.")]
        [Display(Name = "Issue Date")]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Brief Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Credential URL (Optional)")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [StringLength(256)]
        public string? CredentialUrl { get; set; }

        public int DisplayOrder { get; set; }
    }
}

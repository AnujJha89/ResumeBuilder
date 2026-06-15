using PuppeteerSharp;
using PuppeteerSharp.Media;
using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System;

namespace ResumeBuilder.Infrastructure.Services
{
    /// <summary>
    /// Implements PDF generation using PuppeteerSharp (headless Chrome).
    ///
    /// How it works (beginner explanation):
    /// 1. PuppeteerSharp downloads a local copy of Chromium (a browser engine).
    /// 2. We open a headless (invisible) browser tab and load our resume HTML string.
    /// 3. Chrome renders the HTML + CSS exactly as it would in the browser.
    /// 4. We tell Chrome to print the page to PDF.
    /// 5. We get back the PDF bytes and return them.
    ///
    /// This approach gives us a WYSIWYG PDF — What You See Is What You Get.
    /// The PDF looks exactly like the Preview page, but as a proper document.
    /// </summary>
    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;

        public static string? WebRootPath { get; set; }

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GeneratePdfAsync(Resume resume)
        {
            try
            {
                _logger.LogInformation("Starting PDF generation for resume ID: {ResumeId}", resume.Id);

                // Step 1: Ensure Chromium is ready (use pre-installed path if provided, otherwise download)
                var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");
                if (string.IsNullOrEmpty(executablePath))
                {
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _logger.LogInformation("Chromium browser downloaded and ready.");
                }
                else
                {
                    _logger.LogInformation("Using pre-installed Chromium at: {Path}", executablePath);
                }

                // Step 2: Build the HTML string for the resume
                var htmlContent = BuildResumeHtml(resume);

                // Step 3: Launch headless Chrome
                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
                };
                if (!string.IsNullOrEmpty(executablePath))
                {
                    launchOptions.ExecutablePath = executablePath;
                }

                await using var browser = await Puppeteer.LaunchAsync(launchOptions);

                // Step 4: Open a new browser page (like a tab)
                await using var page = await browser.NewPageAsync();

                // Step 5: Set the HTML content of the page
                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }
                });

                // Step 6: Generate PDF with A4 page size and proper margins
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "15mm",
                        Bottom = "15mm",
                        Left = "15mm",
                        Right = "15mm"
                    }
                });

                _logger.LogInformation("PDF generated successfully for resume ID: {ResumeId}, Size: {Size} bytes", resume.Id, pdfBytes.Length);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF generation failed for resume ID: {ResumeId}", resume.Id);
                throw new InvalidOperationException($"PDF generation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Builds a complete, self-contained HTML document string for the resume.
        /// All CSS is embedded inline so no external files are needed.
        /// This is a professional single-column ATS-friendly layout.
        /// Made internal for unit testing via InternalsVisibleTo.
        /// </summary>
        string IPdfService.BuildResumeHtml(Resume resume)
        {
            return BuildResumeHtml(resume);
        }

        internal static string BuildResumeHtml(Resume resume)
        {
            // Determine template name based on TemplateId to avoid EF Core stale navigation property name tracking
            string templateName = resume.TemplateId switch
            {
                1 => "Classic Minimalist",
                2 => "Modern Professional",
                3 => "Executive Elegant",
                4 => "Tech Developer",
                5 => "Creative Portfolio",
                6 => "Academic CV",
                7 => "Stylish Minimal",
                _ => resume.Template?.Name ?? "Classic Minimalist"
            };

            switch (templateName)
            {
                case "Modern Professional":
                    return BuildModernProfessionalHtml(resume);
                case "Executive Elegant":
                    return BuildExecutiveElegantHtml(resume);
                case "Tech Developer":
                    return BuildTechDeveloperHtml(resume);
                case "Creative Portfolio":
                    return BuildCreativePortfolioHtml(resume);
                case "Academic CV":
                    return BuildAcademicCvHtml(resume);
                case "Stylish Minimal":
                    return BuildStylishMinimalHtml(resume);
                case "Classic Minimalist":
                default:
                    return BuildClassicMinimalistHtml(resume);
            }
        }

        private static string BuildTemplateHtml(
            Resume resume,
            string fontStyles,
            string cssStyles,
            Func<Resume, string> bodyRenderer)
        {
            var bodyHtml = bodyRenderer(resume);
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{HtmlEncode(resume.FullName)} - Resume</title>
    <style>
        {fontStyles}
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            line-height: 1.5;
            background: #ffffff;
        }}
        {cssStyles}
        @media print {{
            body {{ -webkit-print-color-adjust: exact; print-color-adjust: exact; }}
        }}
        @page {{ size: A4; margin: 0; }}
    </style>
</head>
<body>
    {bodyHtml}
</body>
</html>";
        }

        #region Helper Renderers

        private static string RenderSummary(string summary, string titleClass = "section-title", string textClass = "summary-text")
        {
            if (string.IsNullOrEmpty(summary)) return "";
            return $@"
            <div class=""section"">
                <div class=""{titleClass}"">Professional Summary</div>
                <div class=""{textClass}"">{HtmlEncode(summary)}</div>
            </div>";
        }

        #endregion

        #region Template 1: Classic Minimalist

        private static string BuildClassicMinimalistHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,600;0,700;1,400&family=Inter:wght@400;500;600&display=swap');",
                @"
                body {
                    font-family: 'Inter', sans-serif;
                    font-size: 10.5pt;
                    color: #111827;
                    padding: 40px;
                }
                .header {
                    text-align: center;
                    margin-bottom: 25px;
                }
                .header h1 {
                    font-family: 'Lora', serif;
                    font-size: 26pt;
                    font-weight: 700;
                    color: #111827;
                    margin-bottom: 6px;
                }
                .job-title {
                    font-size: 12pt;
                    color: #4b5563;
                    font-weight: 500;
                    margin-bottom: 10px;
                }
                .contact-info {
                    font-size: 9.5pt;
                    color: #4b5563;
                }
                .contact-info span {
                    display: inline-block;
                    margin: 0 6px;
                }
                .contact-info a {
                    color: #111827;
                    text-decoration: underline;
                }
                .section {
                    margin-bottom: 20px;
                }
                .section-title {
                    font-family: 'Lora', serif;
                    font-size: 13pt;
                    font-weight: 700;
                    color: #111827;
                    text-transform: uppercase;
                    border-bottom: 1px solid #111827;
                    padding-bottom: 3px;
                    margin-bottom: 12px;
                    letter-spacing: 0.5px;
                }
                .item-title-row {
                    display: flex;
                    justify-content: space-between;
                    align-items: baseline;
                    margin-bottom: 2px;
                }
                .item-title {
                    font-weight: 600;
                    font-size: 11pt;
                }
                .item-subtitle-row {
                    display: flex;
                    justify-content: space-between;
                    font-size: 9.5pt;
                    color: #4b5563;
                    margin-bottom: 4px;
                }
                .item-desc {
                    font-size: 10pt;
                    color: #374151;
                    white-space: pre-line;
                    margin-bottom: 8px;
                }
                .skills-cat {
                    margin-bottom: 6px;
                    font-size: 10pt;
                }
                .skills-cat strong {
                    font-weight: 600;
                }
                ",
                r => $@"
                <div class=""header"">
                    <h1>{HtmlEncode(r.FullName)}</h1>
                    {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}
                    <div class=""contact-info"">
                        {(!string.IsNullOrEmpty(r.Email) ? $"<span>{HtmlEncode(r.Email)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.Phone) ? $"<span>{HtmlEncode(r.Phone)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.Address) ? $"<span>{HtmlEncode(r.Address)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<span><a href=\"{HtmlEncode(r.GitHubUrl)}\">GitHub</a></span>" : "")}
                        {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<span><a href=\"{HtmlEncode(r.LinkedInUrl)}\">LinkedIn</a></span>" : "")}
                    </div>
                </div>

                {RenderSummary(r.ProfessionalSummary)}

                {RenderClassicMinimalistExperience(r.Experiences)}
                {RenderClassicMinimalistProjects(r.Projects)}
                {RenderClassicMinimalistSkills(r.Skills)}
                {RenderClassicMinimalistEducation(r.Educations)}
                {RenderClassicMinimalistCertifications(r.Certifications)}
                {RenderClassicMinimalistAchievements(r.Achievements)}
                "
            );
        }

        private static string RenderClassicMinimalistExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(exp.Position)}</span>
                        <span style=""font-size: 9.5pt; color: #4b5563;"">{exp.StartDate:MMM yyyy} – {end}</span>
                    </div>
                    <div class=""item-subtitle-row"">
                        <span>{HtmlEncode(exp.CompanyName)}</span>
                    </div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Experience</div>{sb}</div>";
        }

        private static string RenderClassicMinimalistProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(proj.ProjectTitle)}</span>
                        <span style=""font-size: 9.5pt; color: #4b5563;"">{HtmlEncode(proj.Duration ?? "")}</span>
                    </div>
                    {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"<div style=\"font-size: 9.5pt; color: #4b5563; margin-bottom: 2px;\">Tech: {HtmlEncode(proj.TechnologiesUsed)}</div>" : "")}
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Projects</div>{sb}</div>";
        }

        private static string RenderClassicMinimalistSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-cat""><strong>{HtmlEncode(cat)}:</strong> {values}</div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Technical Skills</div>{sb}</div>" : "";
        }

        private static string RenderClassicMinimalistEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(edu.Degree)}</span>
                        <span style=""font-size: 9.5pt; color: #4b5563;"">{edu.PassingYear}</span>
                    </div>
                    <div class=""item-subtitle-row"">
                        <span>{HtmlEncode(edu.InstitutionName)}{(string.IsNullOrEmpty(edu.BoardOrUniversity) ? "" : " · " + HtmlEncode(edu.BoardOrUniversity))}</span>
                        <span>{HtmlEncode(edu.PercentageOrCGPA ?? "")}</span>
                    </div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderClassicMinimalistCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"" style=""margin-bottom: 6px;"">
                    <div class=""item-title-row"">
                        <span class=""item-title"" style=""font-size: 10pt;"">{HtmlEncode(cert.CertificateName)}</span>
                        <span style=""font-size: 9.5pt; color: #4b5563;"">{cert.IssueDate:MMM yyyy}</span>
                    </div>
                    <div style=""font-size: 9.5pt; color: #4b5563;"">{HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderClassicMinimalistAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 6px; font-size: 10pt;"">
                    <strong>★ {HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? "– " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        #endregion

        #region Template 2: Modern Professional

        private static string BuildModernProfessionalHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap');",
                @"
                body {
                    font-family: 'Inter', sans-serif;
                    font-size: 10pt;
                    color: #1e293b;
                    padding: 35px;
                }
                .header {
                    margin-bottom: 25px;
                }
                .header-container {
                    display: flex;
                    align-items: center;
                    margin-bottom: 25px;
                    gap: 20px;
                }
                .profile-pic-container {
                    flex-shrink: 0;
                }
                .profile-pic-container img {
                    width: 90px;
                    height: 90px;
                    border-radius: 50%;
                    object-fit: cover;
                    border: 3.5px solid #2563eb;
                    box-shadow: 0 4px 10px rgba(37, 99, 235, 0.15);
                }
                .header h1 {
                    font-size: 28pt;
                    font-weight: 800;
                    color: #0f172a;
                    line-height: 1.1;
                }
                .job-title {
                    font-size: 13pt;
                    color: #2563eb;
                    font-weight: 600;
                    margin-top: 4px;
                    margin-bottom: 10px;
                }
                .contact-info {
                    display: flex;
                    flex-wrap: wrap;
                    gap: 10px 16px;
                    font-size: 9pt;
                    color: #475569;
                }
                .contact-info a {
                    color: #2563eb;
                    text-decoration: none;
                }
                .layout-grid {
                    display: grid;
                    grid-template-columns: 1fr 1.6fr;
                    gap: 25px;
                }
                .section {
                    margin-bottom: 22px;
                }
                .section-title {
                    font-size: 10.5pt;
                    font-weight: 700;
                    color: #2563eb;
                    text-transform: uppercase;
                    border-bottom: 2px solid #e2e8f0;
                    padding-bottom: 4px;
                    margin-bottom: 12px;
                    letter-spacing: 1px;
                }
                .item-title {
                    font-weight: 700;
                    color: #0f172a;
                    font-size: 10pt;
                }
                .item-meta {
                    font-size: 8.5pt;
                    color: #64748b;
                    margin-bottom: 3px;
                }
                .item-desc {
                    font-size: 9.5pt;
                    color: #334155;
                    white-space: pre-line;
                    margin-bottom: 10px;
                }
                .sidebar-item {
                    margin-bottom: 12px;
                }
                .skills-group {
                    margin-bottom: 8px;
                }
                .skills-group strong {
                    font-weight: 600;
                    font-size: 9.5pt;
                    color: #1e293b;
                }
                .skills-group span {
                    font-size: 9pt;
                    color: #475569;
                }
                ",
                r => {
                    var profilePicHtml = RenderProfilePicture(r);
                    var headerStyle = string.IsNullOrEmpty(profilePicHtml)
                        ? "border-left: 6px solid #2563eb; padding-left: 20px; margin-bottom: 25px;"
                        : "border-left: none; padding-left: 0; flex-grow: 1; margin-bottom: 0;";

                    return $@"
                    <div class=""header-container"">
                        {profilePicHtml}
                        <div class=""header"" style=""{headerStyle}"">
                            <h1>{HtmlEncode(r.FullName)}</h1>
                            {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}
                            <div class=""contact-info"">
                                {(!string.IsNullOrEmpty(r.Email) ? $"<span>✉ {HtmlEncode(r.Email)}</span>" : "")}
                                {(!string.IsNullOrEmpty(r.Phone) ? $"<span>📞 {HtmlEncode(r.Phone)}</span>" : "")}
                                {(!string.IsNullOrEmpty(r.Address) ? $"<span>📍 {HtmlEncode(r.Address)}</span>" : "")}
                                {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<span><a href=\"{HtmlEncode(r.GitHubUrl)}\">GitHub</a></span>" : "")}
                                {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<span><a href=\"{HtmlEncode(r.LinkedInUrl)}\">LinkedIn</a></span>" : "")}
                            </div>
                        </div>
                    </div>

                    <div class=""layout-grid"">
                        <!-- Left column -->
                        <div>
                            {RenderModernSidebarEducation(r.Educations)}
                            {RenderModernSidebarSkills(r.Skills)}
                            {RenderModernSidebarCertifications(r.Certifications)}
                            {RenderModernSidebarAchievements(r.Achievements)}
                        </div>
                        <!-- Right column -->
                        <div>
                            {RenderSummary(r.ProfessionalSummary)}
                            {RenderModernMainExperience(r.Experiences)}
                            {RenderModernMainProjects(r.Projects)}
                        </div>
                    </div>
                    ";
                }
            );
        }

        private static string RenderProfilePicture(Resume resume)
        {
            if (string.IsNullOrEmpty(resume.ProfilePicturePath))
            {
                return "";
            }

            var src = resume.ProfilePicturePath;
            if (!string.IsNullOrEmpty(WebRootPath))
            {
                var cleanPath = resume.ProfilePicturePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                var physicalPath = Path.Combine(WebRootPath, cleanPath);
                if (File.Exists(physicalPath))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(physicalPath);
                        var base64 = Convert.ToBase64String(bytes);
                        var ext = Path.GetExtension(physicalPath).ToLower().Replace(".", "");
                        var mime = ext == "png" ? "image/png" : ext == "gif" ? "image/gif" : "image/jpeg";
                        src = $"data:{mime};base64,{base64}";
                    }
                    catch
                    {
                        // Fallback to relative path
                    }
                }
            }

            return $@"
            <div class=""profile-pic-container"">
                <img src=""{src}"" alt=""Profile Picture"" />
            </div>";
        }

        private static string RenderModernSidebarEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div class=""sidebar-item"">
                    <div class=""item-title"">{HtmlEncode(edu.Degree)}</div>
                    <div class=""item-meta"">{HtmlEncode(edu.InstitutionName)} | {edu.PassingYear}</div>
                    <div style=""font-size: 9pt; color: #64748b;"">Score: {HtmlEncode(edu.PercentageOrCGPA ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderModernSidebarSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-group""><strong>{HtmlEncode(cat)}:</strong> <span>{values}</span></div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Technical Skills</div>{sb}</div>" : "";
        }

        private static string RenderModernSidebarCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div class=""sidebar-item"">
                    <div class=""item-title"" style=""font-size: 9.5pt;"">{HtmlEncode(cert.CertificateName)}</div>
                    <div class=""item-meta"">{HtmlEncode(cert.IssuingOrganization)} · {cert.IssueDate:MMM yyyy}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderModernSidebarAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div class=""sidebar-item"" style=""font-size: 9pt;"">
                    <strong>★ {HtmlEncode(ach.Title)}</strong>
                    {(!string.IsNullOrEmpty(ach.Description) ? $"<div style=\"color:#475569;\">{HtmlEncode(ach.Description)}</div>" : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        private static string RenderModernMainExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div style=""margin-bottom: 15px;"">
                    <div class=""item-title"">{HtmlEncode(exp.Position)}</div>
                    <div class=""item-meta"" style=""font-weight: 500; color: #2563eb;"">{HtmlEncode(exp.CompanyName)} | {exp.StartDate:MMM yyyy} – {end}</div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Experience</div>{sb}</div>";
        }

        private static string RenderModernMainProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 15px;"">
                    <div class=""item-title"">{HtmlEncode(proj.ProjectTitle)}</div>
                    <div class=""item-meta"">{HtmlEncode(proj.Duration ?? "")} {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"· Tech: {HtmlEncode(proj.TechnologiesUsed)}" : "")}</div>
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Projects</div>{sb}</div>";
        }

        #endregion

        #region Template 3: Executive Elegant

        private static string BuildExecutiveElegantHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,600;0,700;1,400&family=Inter:wght@400;500;600&display=swap');",
                @"
                body {
                    font-family: 'Inter', sans-serif;
                    font-size: 10.5pt;
                    color: #1e293b;
                    padding: 45px;
                }
                .header {
                    text-align: center;
                    margin-bottom: 28px;
                }
                .header h1 {
                    font-family: 'Lora', serif;
                    font-size: 26pt;
                    font-weight: 500;
                    color: #1e3a8a;
                    text-transform: uppercase;
                    letter-spacing: 1.5px;
                    margin-bottom: 4px;
                }
                .job-title {
                    font-family: 'Lora', serif;
                    font-size: 12pt;
                    color: #b45309;
                    font-weight: 600;
                    text-transform: uppercase;
                    letter-spacing: 2.5px;
                    margin-bottom: 10px;
                }
                .contact-info {
                    font-size: 9.5pt;
                    color: #475569;
                    letter-spacing: 0.5px;
                }
                .contact-info span {
                    display: inline-block;
                    margin: 0 8px;
                }
                .contact-info a {
                    color: #1e3a8a;
                    text-decoration: none;
                    border-bottom: 1px solid #b45309;
                }
                .section {
                    margin-bottom: 24px;
                }
                .section-title {
                    font-family: 'Lora', serif;
                    font-size: 11.5pt;
                    font-weight: 700;
                    color: #1e3a8a;
                    text-transform: uppercase;
                    border-bottom: 1.5px solid #c59d5f;
                    padding-bottom: 3px;
                    margin-bottom: 14px;
                    letter-spacing: 1.2px;
                }
                .item-title-row {
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 3px;
                }
                .item-title {
                    font-family: 'Lora', serif;
                    font-weight: 700;
                    color: #0f172a;
                    font-size: 11pt;
                }
                .item-subtitle {
                    font-size: 9.5pt;
                    color: #475569;
                    font-style: italic;
                }
                .item-desc {
                    font-size: 10pt;
                    color: #334155;
                    white-space: pre-line;
                    margin-bottom: 10px;
                    padding-left: 10px;
                    border-left: 2px solid #e2e8f0;
                }
                .skills-row {
                    margin-bottom: 6px;
                }
                .skills-row strong {
                    color: #1e3a8a;
                    font-family: 'Lora', serif;
                    font-size: 10.5pt;
                }
                ",
                r => $@"
                <div class=""header"">
                    <h1>{HtmlEncode(r.FullName)}</h1>
                    {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}
                    <div class=""contact-info"">
                        {(!string.IsNullOrEmpty(r.Email) ? $"<span>{HtmlEncode(r.Email)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.Phone) ? $"<span>{HtmlEncode(r.Phone)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.Address) ? $"<span>{HtmlEncode(r.Address)}</span>" : "")}
                        {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<span><a href=\"{HtmlEncode(r.GitHubUrl)}\">GitHub</a></span>" : "")}
                        {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<span><a href=\"{HtmlEncode(r.LinkedInUrl)}\">LinkedIn</a></span>" : "")}
                    </div>
                </div>

                {RenderSummary(r.ProfessionalSummary, "section-title")}

                {RenderExecutiveExperience(r.Experiences)}
                {RenderExecutiveProjects(r.Projects)}
                {RenderExecutiveSkills(r.Skills)}
                {RenderExecutiveEducation(r.Educations)}
                {RenderExecutiveCertifications(r.Certifications)}
                {RenderExecutiveAchievements(r.Achievements)}
                "
            );
        }

        private static string RenderExecutiveExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div style=""margin-bottom: 16px;"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(exp.Position)}</span>
                        <span style=""font-family: 'Lora', serif; font-size: 9.5pt; color: #b45309; font-weight: 600;"">{exp.StartDate:MMM yyyy} – {end}</span>
                    </div>
                    <div class=""item-subtitle"" style=""margin-bottom: 6px;"">{HtmlEncode(exp.CompanyName)}</div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Professional Experience</div>{sb}</div>";
        }

        private static string RenderExecutiveProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 16px;"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(proj.ProjectTitle)}</span>
                        <span style=""font-family: 'Lora', serif; font-size: 9.5pt; color: #b45309; font-weight: 600;"">{HtmlEncode(proj.Duration ?? "")}</span>
                    </div>
                    {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"<div class=\"item-subtitle\" style=\"margin-bottom:4px;\">Technologies: {HtmlEncode(proj.TechnologiesUsed)}</div>" : "")}
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Key Projects</div>{sb}</div>";
        }

        private static string RenderExecutiveSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-row""><strong>{HtmlEncode(cat)}: </strong>{values}</div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Core Competencies</div>{sb}</div>" : "";
        }

        private static string RenderExecutiveEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 12px;"">
                    <div class=""item-title-row"">
                        <span class=""item-title"">{HtmlEncode(edu.Degree)}</span>
                        <span style=""font-family: 'Lora', serif; font-size: 9.5pt; color: #b45309; font-weight: 600;"">{edu.PassingYear}</span>
                    </div>
                    <div class=""item-subtitle"">{HtmlEncode(edu.InstitutionName)}{(string.IsNullOrEmpty(edu.BoardOrUniversity) ? "" : " · " + HtmlEncode(edu.BoardOrUniversity))} | Score: {HtmlEncode(edu.PercentageOrCGPA ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderExecutiveCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 10px;"">
                    <div class=""item-title-row"">
                        <span class=""item-title"" style=""font-size: 10pt;"">{HtmlEncode(cert.CertificateName)}</span>
                        <span style=""font-family: 'Lora', serif; font-size: 9pt; color: #b45309; font-weight:600;"">{cert.IssueDate:MMM yyyy}</span>
                    </div>
                    <div class=""item-subtitle"">{HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications & Awards</div>{sb}</div>";
        }

        private static string RenderExecutiveAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 8px; font-size: 10pt;"">
                    <strong style=""color: #0f172a;"">★ {HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? "— " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        #endregion

        #region Template 4: Tech Developer

        private static string BuildTechDeveloperHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Inter:wght@400;500;600;700;800&display=swap');",
                @"
                body {
                    font-family: 'Inter', sans-serif;
                    font-size: 10pt;
                    color: #0f172a;
                    padding: 30px;
                }
                .header {
                    display: flex;
                    justify-content: space-between;
                    align-items: flex-end;
                    border-bottom: 2px solid #0f172a;
                    padding-bottom: 12px;
                    margin-bottom: 20px;
                }
                .header-left h1 {
                    font-size: 26pt;
                    font-weight: 800;
                    color: #0f172a;
                    line-height: 1.0;
                }
                .job-title {
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 12pt;
                    color: #059669;
                    font-weight: 700;
                    margin-top: 6px;
                }
                .contact-info {
                    text-align: right;
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 8.5pt;
                    color: #4b5563;
                    line-height: 1.5;
                }
                .contact-info a {
                    color: #059669;
                    text-decoration: none;
                }
                .section {
                    margin-bottom: 18px;
                }
                .section-title {
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 10.5pt;
                    font-weight: 700;
                    color: #0f172a;
                    background: #f3f4f6;
                    padding: 4px 10px;
                    border-left: 5px solid #059669;
                    margin-bottom: 12px;
                    text-transform: uppercase;
                }
                .item-title-row {
                    display: flex;
                    justify-content: space-between;
                    font-size: 10pt;
                    font-weight: 700;
                    color: #0f172a;
                    margin-bottom: 2px;
                }
                .item-tech-stack {
                    font-family: 'JetBrains Mono', monospace;
                    font-size: 8.5pt;
                    color: #059669;
                    margin-bottom: 4px;
                }
                .item-desc {
                    font-size: 9.5pt;
                    color: #374151;
                    white-space: pre-line;
                    margin-bottom: 8px;
                }
                .skill-badge {
                    display: inline-block;
                    background: #f3f4f6;
                    border: 1px solid #e5e7eb;
                    border-radius: 4px;
                    padding: 2px 8px;
                    font-size: 8.5pt;
                    margin: 2px;
                    font-family: 'JetBrains Mono', monospace;
                    color: #1f2937;
                }
                .skills-container {
                    display: flex;
                    flex-direction: column;
                    gap: 6px;
                }
                ",
                r => $@"
                <div class=""header"">
                    <div class=""header-left"">
                        <h1>{HtmlEncode(r.FullName)}</h1>
                        {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">&lt; {HtmlEncode(r.JobTitle)} /&gt;</div>" : "")}
                    </div>
                    <div class=""contact-info"">
                        {(!string.IsNullOrEmpty(r.Email) ? $"<div>email: <a href=\"mailto:{HtmlEncode(r.Email)}\">{HtmlEncode(r.Email)}</a></div>" : "")}
                        {(!string.IsNullOrEmpty(r.Phone) ? $"<div>phone: {HtmlEncode(r.Phone)}</div>" : "")}
                        {(!string.IsNullOrEmpty(r.Address) ? $"<div>loc: {HtmlEncode(r.Address)}</div>" : "")}
                        {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<div>github: <a href=\"{HtmlEncode(r.GitHubUrl)}\">{HtmlEncode(r.GitHubUrl.Replace("https://", ""))}</a></div>" : "")}
                        {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<div>linkedin: <a href=\"{HtmlEncode(r.LinkedInUrl)}\">{HtmlEncode(r.LinkedInUrl.Replace("https://", ""))}</a></div>" : "")}
                    </div>
                </div>

                {RenderSummary(r.ProfessionalSummary, "section-title", "item-desc")}

                {RenderTechExperience(r.Experiences)}
                {RenderTechProjects(r.Projects)}
                {RenderTechSkills(r.Skills)}
                {RenderTechEducation(r.Educations)}
                {RenderTechCertifications(r.Certifications)}
                {RenderTechAchievements(r.Achievements)}
                "
            );
        }

        private static string RenderTechExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div style=""margin-bottom: 12px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(exp.Position)} @ {HtmlEncode(exp.CompanyName)}</span>
                        <span style=""font-family: 'JetBrains Mono', monospace; font-size: 8.5pt; color: #4b5563; font-weight:normal;"">[{exp.StartDate:MM/yyyy} - {end}]</span>
                    </div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Experience</div>{sb}</div>";
        }

        private static string RenderTechProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 12px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(proj.ProjectTitle)}</span>
                        <span style=""font-family: 'JetBrains Mono', monospace; font-size: 8.5pt; color: #4b5563; font-weight:normal;"">[{HtmlEncode(proj.Duration ?? "")}]</span>
                    </div>
                    {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"<div class=\"item-tech-stack\">&gt; build --using: {HtmlEncode(proj.TechnologiesUsed)}</div>" : "")}
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Projects</div>{sb}</div>";
        }

        private static string RenderTechSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var badges = new System.Text.StringBuilder();
                foreach (var s in catSkills)
                {
                    var prof = string.IsNullOrEmpty(s.Proficiency) ? "" : $" [{HtmlEncode(s.Proficiency)}]";
                    badges.Append($@"<span class=""skill-badge"">{HtmlEncode(s.SkillName)}{prof}</span>");
                }
                sb.Append($@"
                <div style=""margin-bottom: 6px;"">
                    <span style=""font-family: 'JetBrains Mono', monospace; font-size: 9pt; font-weight:600; width: 100px; display:inline-block;"">{HtmlEncode(cat)}:</span>
                    {badges}
                </div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Technical Skills</div><div class=""skills-container"">{sb}</div></div>" : "";
        }

        private static string RenderTechEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 8px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(edu.Degree)}</span>
                        <span style=""font-family: 'JetBrains Mono', monospace; font-size: 8.5pt; color: #4b5563; font-weight:normal;"">[{edu.PassingYear}]</span>
                    </div>
                    <div style=""font-size: 9pt; color: #4b5563;"">{HtmlEncode(edu.InstitutionName)} | GPA/Score: {HtmlEncode(edu.PercentageOrCGPA ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderTechCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 6px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(cert.CertificateName)}</span>
                        <span style=""font-family: 'JetBrains Mono', monospace; font-size: 8.5pt; color: #4b5563; font-weight:normal;"">[{cert.IssueDate:yyyy}]</span>
                    </div>
                    <div style=""font-size: 9pt; color: #4b5563;"">Issuer: {HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderTechAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""font-family: 'JetBrains Mono', monospace; font-size: 9pt; margin-bottom: 4px;"">
                    <span style=""color: #059669;"">*</span> <strong>{HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? ":: " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        #endregion

        #region Template 5: Creative Portfolio

        private static string BuildCreativePortfolioHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700;800&display=swap');",
                @"
                body {
                    font-family: 'Poppins', sans-serif;
                    font-size: 10pt;
                    color: #334155;
                    padding: 0;
                }
                .page-container {
                    display: flex;
                    min-height: 100vh;
                }
                .sidebar {
                    width: 32%;
                    background: #fdf2f8;
                    border-right: 1px solid #fbcfe8;
                    padding: 35px 20px;
                }
                .main-content {
                    width: 68%;
                    padding: 35px 25px;
                }
                .name {
                    font-size: 22pt;
                    font-weight: 800;
                    color: #db2777;
                    line-height: 1.1;
                    margin-bottom: 5px;
                }
                .job-title {
                    font-size: 11pt;
                    color: #06b6d4;
                    font-weight: 600;
                    margin-bottom: 20px;
                }
                .contact-block {
                    font-size: 8.5pt;
                    color: #475569;
                    line-height: 1.6;
                    margin-bottom: 25px;
                }
                .contact-block div {
                    margin-bottom: 6px;
                }
                .contact-block a {
                    color: #db2777;
                    text-decoration: none;
                }
                .sidebar .section-title {
                    font-size: 10pt;
                    font-weight: 700;
                    color: #db2777;
                    text-transform: uppercase;
                    border-bottom: 2px solid #fbcfe8;
                    padding-bottom: 4px;
                    margin-bottom: 12px;
                    margin-top: 25px;
                }
                .main-content .section-title {
                    font-size: 11pt;
                    font-weight: 700;
                    color: #06b6d4;
                    text-transform: uppercase;
                    border-bottom: 2px solid #cffafe;
                    padding-bottom: 4px;
                    margin-bottom: 12px;
                    margin-top: 20px;
                }
                .item-title {
                    font-weight: 600;
                    color: #1e293b;
                    font-size: 10pt;
                }
                .item-subtitle {
                    font-size: 8.5pt;
                    color: #db2777;
                    font-weight: 500;
                }
                .item-desc {
                    font-size: 9pt;
                    color: #475569;
                    white-space: pre-line;
                    margin-bottom: 10px;
                }
                .sidebar-item {
                    margin-bottom: 12px;
                }
                .skills-group {
                    margin-bottom: 8px;
                    font-size: 9pt;
                }
                .skills-group strong {
                    color: #db2777;
                }
                ",
                r => $@"
                <div class=""page-container"">
                    <!-- Left Sidebar -->
                    <div class=""sidebar"">
                        <div class=""name"">{HtmlEncode(r.FullName)}</div>
                        {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}

                        <div class=""contact-block"">
                            {(!string.IsNullOrEmpty(r.Email) ? $"<div>✉ <a href=\"mailto:{HtmlEncode(r.Email)}\">{HtmlEncode(r.Email)}</a></div>" : "")}
                            {(!string.IsNullOrEmpty(r.Phone) ? $"<div>📞 {HtmlEncode(r.Phone)}</div>" : "")}
                            {(!string.IsNullOrEmpty(r.Address) ? $"<div>📍 {HtmlEncode(r.Address)}</div>" : "")}
                            {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<div>🕸 <a href=\"{HtmlEncode(r.GitHubUrl)}\">GitHub</a></div>" : "")}
                            {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<div>🕸 <a href=\"{HtmlEncode(r.LinkedInUrl)}\">LinkedIn</a></div>" : "")}
                        </div>

                        {RenderCreativeSidebarSkills(r.Skills)}
                        {RenderCreativeSidebarEducation(r.Educations)}
                        {RenderCreativeSidebarCertifications(r.Certifications)}
                    </div>

                    <!-- Right Main Content -->
                    <div class=""main-content"">
                        {RenderSummary(r.ProfessionalSummary, "section-title", "item-desc")}
                        {RenderCreativeMainExperience(r.Experiences)}
                        {RenderCreativeMainProjects(r.Projects)}
                        {RenderCreativeMainAchievements(r.Achievements)}
                    </div>
                </div>
                "
            );
        }

        private static string RenderCreativeSidebarSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-group""><strong>{HtmlEncode(cat)}:</strong><br/>{values}</div>");
            }
            return sb.Length > 0 ? $@"<div><div class=""section-title"">Skills</div>{sb}</div>" : "";
        }

        private static string RenderCreativeSidebarEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div class=""sidebar-item"">
                    <div class=""item-title"" style=""font-size:9pt;"">{HtmlEncode(edu.Degree)}</div>
                    <div class=""item-subtitle"">{edu.PassingYear} · {HtmlEncode(edu.InstitutionName)}</div>
                </div>");
            }
            return $@"<div><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderCreativeSidebarCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div class=""sidebar-item"">
                    <div class=""item-title"" style=""font-size:9pt;"">{HtmlEncode(cert.CertificateName)}</div>
                    <div class=""item-subtitle"">{HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderCreativeMainExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div style=""margin-bottom: 14px;"">
                    <div class=""item-title"">{HtmlEncode(exp.Position)}</div>
                    <div class=""item-subtitle"" style=""color:#06b6d4;"">{HtmlEncode(exp.CompanyName)} | {exp.StartDate:MMM yyyy} – {end}</div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Experience</div>{sb}</div>";
        }

        private static string RenderCreativeMainProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 14px;"">
                    <div class=""item-title"">{HtmlEncode(proj.ProjectTitle)}</div>
                    <div class=""item-subtitle"" style=""color:#06b6d4;"">{HtmlEncode(proj.Duration ?? "")} {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"· Tech: {HtmlEncode(proj.TechnologiesUsed)}" : "")}</div>
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Featured Projects</div>{sb}</div>";
        }

        private static string RenderCreativeMainAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 8px; font-size: 9.5pt;"">
                    <strong style=""color: #db2777;"">✦ {HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? "– " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        #endregion

        #region Template 6: Academic CV

        private static string BuildAcademicCvHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,600;0,700;1,400&display=swap');",
                @"
                body {
                    font-family: 'Lora', Georgia, serif;
                    font-size: 10.5pt;
                    color: #111111;
                    padding: 45px;
                    line-height: 1.5;
                }
                .header {
                    margin-bottom: 25px;
                }
                .header h1 {
                    font-size: 24pt;
                    font-weight: bold;
                    color: #000000;
                    margin-bottom: 4px;
                }
                .job-title {
                    font-size: 12pt;
                    font-style: italic;
                    color: #333333;
                    margin-bottom: 10px;
                }
                .contact-info {
                    font-size: 9.5pt;
                    color: #333333;
                    line-height: 1.4;
                }
                .contact-info a {
                    color: #000;
                    text-decoration: underline;
                }
                .section {
                    margin-bottom: 22px;
                }
                .section-title {
                    font-size: 12pt;
                    font-weight: bold;
                    border-bottom: 1px solid #888888;
                    padding-bottom: 2px;
                    margin-top: 20px;
                    margin-bottom: 10px;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                }
                .item-row {
                    margin-bottom: 12px;
                }
                .item-header {
                    font-weight: bold;
                    display: flex;
                    justify-content: space-between;
                }
                .item-subheader {
                    font-style: italic;
                    color: #444444;
                    margin-bottom: 3px;
                }
                .item-desc {
                    font-size: 9.5pt;
                    color: #222222;
                    white-space: pre-line;
                    margin-top: 2px;
                }
                .skills-row {
                    font-size: 10pt;
                    margin-bottom: 5px;
                }
                ",
                r => $@"
                <div class=""header"">
                    <h1>{HtmlEncode(r.FullName)}</h1>
                    {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}
                    <div class=""contact-info"">
                        {(!string.IsNullOrEmpty(r.Email) ? $"<div>Email: {HtmlEncode(r.Email)}</div>" : "")}
                        {(!string.IsNullOrEmpty(r.Phone) ? $"<div>Phone: {HtmlEncode(r.Phone)}</div>" : "")}
                        {(!string.IsNullOrEmpty(r.Address) ? $"<div>Address: {HtmlEncode(r.Address)}</div>" : "")}
                        {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<div>GitHub: <a href=\"{HtmlEncode(r.GitHubUrl)}\">{HtmlEncode(r.GitHubUrl)}</a></div>" : "")}
                        {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<div>LinkedIn: <a href=\"{HtmlEncode(r.LinkedInUrl)}\">{HtmlEncode(r.LinkedInUrl)}</a></div>" : "")}
                    </div>
                </div>

                {RenderSummary(r.ProfessionalSummary)}

                {RenderAcademicEducation(r.Educations)}
                {RenderAcademicExperience(r.Experiences)}
                {RenderAcademicProjects(r.Projects)}
                {RenderAcademicSkills(r.Skills)}
                {RenderAcademicCertifications(r.Certifications)}
                {RenderAcademicAchievements(r.Achievements)}
                "
            );
        }

        private static string RenderAcademicEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-header"">
                        <span>{HtmlEncode(edu.Degree)}</span>
                        <span>{edu.PassingYear}</span>
                    </div>
                    <div class=""item-subheader"">{HtmlEncode(edu.InstitutionName)}{(string.IsNullOrEmpty(edu.BoardOrUniversity) ? "" : " · " + HtmlEncode(edu.BoardOrUniversity))} | Score: {HtmlEncode(edu.PercentageOrCGPA ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderAcademicExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-header"">
                        <span>{HtmlEncode(exp.Position)}</span>
                        <span>{exp.StartDate:MMM yyyy} – {end}</span>
                    </div>
                    <div class=""item-subheader"">{HtmlEncode(exp.CompanyName)}</div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Professional Appointments</div>{sb}</div>";
        }

        private static string RenderAcademicProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"">
                    <div class=""item-header"">
                        <span>{HtmlEncode(proj.ProjectTitle)}</span>
                        <span>{HtmlEncode(proj.Duration ?? "")}</span>
                    </div>
                    {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"<div class=\"item-subheader\">Technologies: {HtmlEncode(proj.TechnologiesUsed)}</div>" : "")}
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Research &amp; Projects</div>{sb}</div>";
        }

        private static string RenderAcademicSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-row""><strong>{HtmlEncode(cat)}:</strong> {values}</div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Technical Competencies</div>{sb}</div>" : "";
        }

        private static string RenderAcademicCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div class=""item-row"" style=""margin-bottom: 8px;"">
                    <div class=""item-header"">
                        <span>{HtmlEncode(cert.CertificateName)}</span>
                        <span>{cert.IssueDate:MMM yyyy}</span>
                    </div>
                    <div class=""item-subheader"">{HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderAcademicAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 6px; font-size: 10pt;"">
                    <strong>{HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? "– " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Honors &amp; Awards</div>{sb}</div>";
        }

        #endregion

        #region Template 7: Stylish Minimal

        private static string BuildStylishMinimalHtml(Resume resume)
        {
            return BuildTemplateHtml(
                resume,
                @"@import url('https://fonts.googleapis.com/css2?family=Montserrat:wght@300;400;500;600;700&family=Inter:wght@300;400;500&display=swap');",
                @"
                body {
                    font-family: 'Inter', sans-serif;
                    font-size: 10pt;
                    color: #4b5563;
                    padding: 40px;
                }
                .header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    border-bottom: 1px solid #e5e7eb;
                    padding-bottom: 20px;
                    margin-bottom: 25px;
                }
                .header h1 {
                    font-family: 'Montserrat', sans-serif;
                    font-size: 24pt;
                    font-weight: 300;
                    color: #111827;
                    letter-spacing: -0.5px;
                    margin: 0;
                }
                .header h1 strong {
                    font-weight: 700;
                    color: #111827;
                }
                .job-title {
                    font-family: 'Montserrat', sans-serif;
                    font-size: 10pt;
                    font-weight: 600;
                    color: #6b7280;
                    text-transform: uppercase;
                    letter-spacing: 2px;
                    margin-top: 2px;
                }
                .contact-info {
                    text-align: right;
                    font-size: 8.5pt;
                    color: #6b7280;
                    line-height: 1.5;
                }
                .contact-info a {
                    color: #111827;
                    text-decoration: none;
                    font-weight: 500;
                }
                .section {
                    margin-bottom: 20px;
                }
                .section-title {
                    font-family: 'Montserrat', sans-serif;
                    font-size: 9pt;
                    font-weight: 700;
                    color: #111827;
                    text-transform: uppercase;
                    letter-spacing: 2px;
                    border-bottom: 1px solid #111827;
                    padding-bottom: 4px;
                    margin-bottom: 12px;
                }
                .item-title-row {
                    display: flex;
                    justify-content: space-between;
                    font-size: 9.5pt;
                    font-weight: 600;
                    color: #111827;
                    margin-bottom: 2px;
                }
                .item-meta {
                    font-size: 8.5pt;
                    color: #9ca3af;
                    margin-bottom: 4px;
                }
                .item-desc {
                    font-size: 9pt;
                    color: #4b5563;
                    white-space: pre-line;
                    margin-bottom: 8px;
                }
                .skills-row {
                    font-size: 9pt;
                    margin-bottom: 4px;
                }
                .skills-row strong {
                    color: #111827;
                    font-family: 'Montserrat', sans-serif;
                    font-size: 8.5pt;
                }
                ",
                r => {
                    var names = (r.FullName ?? "").Split(' ');
                    var styledName = r.FullName;
                    if (names.Length > 1)
                    {
                        var first = string.Join(" ", names.Take(names.Length - 1));
                        var last = names.Last();
                        styledName = $"{first} <strong>{last}</strong>";
                    }
                    return $@"
                    <div class=""header"">
                        <div>
                            <h1>{styledName}</h1>
                            {(!string.IsNullOrEmpty(r.JobTitle) ? $"<div class=\"job-title\">{HtmlEncode(r.JobTitle)}</div>" : "")}
                        </div>
                        <div class=""contact-info"">
                            {(!string.IsNullOrEmpty(r.Email) ? $"<div><a href=\"mailto:{HtmlEncode(r.Email)}\">{HtmlEncode(r.Email)}</a></div>" : "")}
                            {(!string.IsNullOrEmpty(r.Phone) ? $"<div>{HtmlEncode(r.Phone)}</div>" : "")}
                            {(!string.IsNullOrEmpty(r.Address) ? $"<div>{HtmlEncode(r.Address)}</div>" : "")}
                            {(!string.IsNullOrEmpty(r.GitHubUrl) ? $"<div><a href=\"{HtmlEncode(r.GitHubUrl)}\">github.com/{HtmlEncode(r.GitHubUrl.Replace("https://github.com/", "").Trim('/'))}</a></div>" : "")}
                            {(!string.IsNullOrEmpty(r.LinkedInUrl) ? $"<div><a href=\"{HtmlEncode(r.LinkedInUrl)}\">linkedin.com/in/{HtmlEncode(r.LinkedInUrl.Replace("https://www.linkedin.com/in/", "").Replace("https://linkedin.com/in/", "").Trim('/'))}</a></div>" : "")}
                        </div>
                    </div>

                    {RenderSummary(r.ProfessionalSummary, "section-title", "item-desc")}

                    {RenderStylishExperience(r.Experiences)}
                    {RenderStylishProjects(r.Projects)}
                    {RenderStylishSkills(r.Skills)}
                    {RenderStylishEducation(r.Educations)}
                    {RenderStylishCertifications(r.Certifications)}
                    {RenderStylishAchievements(r.Achievements)}
                    ";
                }
            );
        }

        private static string RenderStylishExperience(ICollection<Experience>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var exp in list.OrderBy(e => e.DisplayOrder))
            {
                var end = exp.IsCurrent ? "Present" : exp.EndDate?.ToString("MMM yyyy") ?? "";
                sb.Append($@"
                <div style=""margin-bottom: 10px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(exp.Position)}</span>
                        <span style=""font-weight: 500; color: #9ca3af;"">{exp.StartDate:MMM yyyy} – {end}</span>
                    </div>
                    <div class=""item-meta"">{HtmlEncode(exp.CompanyName)}</div>
                    <div class=""item-desc"">{HtmlEncode(exp.Responsibilities ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Experience</div>{sb}</div>";
        }

        private static string RenderStylishProjects(ICollection<Project>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var proj in list.OrderBy(p => p.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 10px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(proj.ProjectTitle)}</span>
                        <span style=""font-weight: 500; color: #9ca3af;"">{HtmlEncode(proj.Duration ?? "")}</span>
                    </div>
                    {(!string.IsNullOrEmpty(proj.TechnologiesUsed) ? $"<div class=\"item-meta\">Tech: {HtmlEncode(proj.TechnologiesUsed)}</div>" : "")}
                    <div class=""item-desc"">{HtmlEncode(proj.Description ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Projects</div>{sb}</div>";
        }

        private static string RenderStylishSkills(ICollection<Skill>? list)
        {
            if (list == null || !list.Any()) return "";
            var categories = new[] { "Languages", "Frameworks", "Databases", "Tools" };
            var sb = new System.Text.StringBuilder();
            foreach (var cat in categories)
            {
                var catSkills = list.Where(s => s.SkillCategory == cat).OrderBy(s => s.DisplayOrder).ToList();
                if (!catSkills.Any()) continue;
                var values = string.Join(", ", catSkills.Select(s => string.IsNullOrEmpty(s.Proficiency) ? HtmlEncode(s.SkillName) : $"{HtmlEncode(s.SkillName)} ({HtmlEncode(s.Proficiency)})"));
                sb.Append($@"<div class=""skills-row""><strong>{HtmlEncode(cat)}:</strong> {values}</div>");
            }
            return sb.Length > 0 ? $@"<div class=""section""><div class=""section-title"">Skills</div>{sb}</div>" : "";
        }

        private static string RenderStylishEducation(ICollection<Education>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var edu in list.OrderBy(e => e.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 8px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(edu.Degree)}</span>
                        <span style=""font-weight: 500; color: #9ca3af;"">{edu.PassingYear}</span>
                    </div>
                    <div class=""item-meta"">{HtmlEncode(edu.InstitutionName)}{(string.IsNullOrEmpty(edu.BoardOrUniversity) ? "" : " · " + HtmlEncode(edu.BoardOrUniversity))} | Score: {HtmlEncode(edu.PercentageOrCGPA ?? "")}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Education</div>{sb}</div>";
        }

        private static string RenderStylishCertifications(ICollection<Certification>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var cert in list.OrderBy(c => c.DisplayOrder))
            {
                sb.Append($@"
                <div style=""margin-bottom: 6px;"">
                    <div class=""item-title-row"">
                        <span>{HtmlEncode(cert.CertificateName)}</span>
                        <span style=""font-weight: 500; color: #9ca3af;"">{cert.IssueDate:MMM yyyy}</span>
                    </div>
                    <div class=""item-meta"">{HtmlEncode(cert.IssuingOrganization)}</div>
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Certifications</div>{sb}</div>";
        }

        private static string RenderStylishAchievements(ICollection<Achievement>? list)
        {
            if (list == null || !list.Any()) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var ach in list.OrderBy(a => a.DisplayOrder))
            {
                sb.Append($@"
                <div style=""font-size: 9pt; margin-bottom: 4px;"">
                    <strong>★ {HtmlEncode(ach.Title)}</strong> {(!string.IsNullOrEmpty(ach.Description) ? "– " + HtmlEncode(ach.Description) : "")}
                </div>");
            }
            return $@"<div class=""section""><div class=""section-title"">Achievements</div>{sb}</div>";
        }

        #endregion

        /// <summary>
        /// Encodes special HTML characters to prevent XSS and malformed HTML.
        /// For example: "&" becomes "&amp;", "<" becomes "&lt;".
        /// This is critical when user-supplied data is injected into HTML.
        /// Made internal for unit testing.
        /// </summary>
        internal static string HtmlEncode(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text ?? "");
        }
    }
}

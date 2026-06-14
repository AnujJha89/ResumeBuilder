using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResumeBuilder.Core.Entities;
using ResumeBuilder.Core.Interfaces;
using ResumeBuilder.Web.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;

namespace ResumeBuilder.Web.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private readonly IResumeService _resumeService;
        private readonly IRepository<ResumeTemplate> _templateRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPdfService _pdfService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResumeController(
            IResumeService resumeService,
            IRepository<ResumeTemplate> templateRepository,
            UserManager<ApplicationUser> userManager,
            IPdfService pdfService,
            IWebHostEnvironment webHostEnvironment)
        {
            _resumeService = resumeService;
            _templateRepository = templateRepository;
            _userManager = userManager;
            _pdfService = pdfService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Resume/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var templates = await _templateRepository.GetAllAsync();
            var activeTemplates = templates.ToList();

            var viewModel = new ResumeEditorViewModel
            {
                AvailableTemplates = activeTemplates
            };

            return View(viewModel);
        }

        // POST: /Resume/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResumeEditorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                var resume = new Resume
                {
                    UserId = user.Id,
                    TemplateId = model.TemplateId,
                    Title = model.Title,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    GitHubUrl = model.GitHubUrl,
                    LinkedInUrl = model.LinkedInUrl,
                    JobTitle = model.JobTitle,
                    ProfessionalSummary = model.ProfessionalSummary
                };

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }
                    resume.ProfilePicturePath = "/uploads/" + fileName;
                }

                await _resumeService.CreateResumeAsync(resume);
                return RedirectToAction(nameof(Edit), new { id = resume.Id });
            }

            var templates = await _templateRepository.GetAllAsync();
            model.AvailableTemplates = templates.ToList();
            return View(model);
        }

        // GET: /Resume/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _resumeService.GetResumeByIdAsync(id);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            var templates = await _templateRepository.GetAllAsync();
            var activeTemplates = templates.Where(t => t.IsActive).ToList();

            var viewModel = new ResumeEditorViewModel
            {
                Id = resume.Id,
                Title = resume.Title,
                TemplateId = resume.TemplateId,
                FullName = resume.FullName,
                Email = resume.Email,
                Phone = resume.Phone,
                Address = resume.Address,
                GitHubUrl = resume.GitHubUrl,
                LinkedInUrl = resume.LinkedInUrl,
                JobTitle = resume.JobTitle,
                ProfessionalSummary = resume.ProfessionalSummary,
                ProfilePicturePath = resume.ProfilePicturePath,
                AvailableTemplates = activeTemplates
            };

            // Viewbag triggers sidebar highlighters
            ViewBag.ActiveTab = "personal";
            return View(viewModel);
        }

        // POST: /Resume/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResumeEditorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _resumeService.GetResumeByIdAsync(id);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                resume.Title = model.Title;
                resume.TemplateId = model.TemplateId;
                resume.FullName = model.FullName;
                resume.Email = model.Email;
                resume.Phone = model.Phone;
                resume.Address = model.Address;
                resume.GitHubUrl = model.GitHubUrl;
                resume.LinkedInUrl = model.LinkedInUrl;

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    // Delete old picture if it exists
                    if (!string.IsNullOrEmpty(resume.ProfilePicturePath))
                    {
                        var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, resume.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldPath);
                            }
                            catch
                            {
                                // Ignore failure to delete old image
                            }
                        }
                    }

                    resume.ProfilePicturePath = "/uploads/" + fileName;
                }

                await _resumeService.UpdateResumeAsync(resume);
                TempData["SuccessMessage"] = "Personal Information updated successfully!";
                return RedirectToAction(nameof(Summary), new { resumeId = resume.Id });
            }

            var templates = await _templateRepository.GetAllAsync();
            model.AvailableTemplates = templates.Where(t => t.IsActive).ToList();
            ViewBag.ActiveTab = "personal";
            return View(model);
        }

        // DELETE: /Resume/Delete/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _resumeService.GetResumeByIdAsync(id);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            await _resumeService.DeleteResumeAsync(id);
            return Ok();
        }

        // GET: /Resume/Summary/5
        [HttpGet]
        public async Task<IActionResult> Summary(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var viewModel = new SummaryViewModel
            {
                ResumeId = resume.Id,
                JobTitle = resume.JobTitle,
                ProfessionalSummary = resume.ProfessionalSummary
            };

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "summary";
            return View(viewModel);
        }

        // POST: /Resume/Summary/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summary(SummaryViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                resume.JobTitle = model.JobTitle;
                resume.ProfessionalSummary = model.ProfessionalSummary;
                await _resumeService.UpdateResumeAsync(resume);
                TempData["SuccessMessage"] = "Professional Summary saved successfully!";
                return RedirectToAction(nameof(Educations), new { resumeId = resume.Id });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "summary";
            return View(model);
        }

        // GET: /Resume/Educations/5
        [HttpGet]
        public async Task<IActionResult> Educations(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "education";

            return View(resume.Educations.OrderBy(e => e.DisplayOrder).ToList());
        }

        // POST: /Resume/AddEducation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEducation(EducationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var education = new Education
                {
                    ResumeId = model.ResumeId,
                    Degree = model.Degree,
                    InstitutionName = model.InstitutionName,
                    BoardOrUniversity = model.BoardOrUniversity,
                    PassingYear = model.PassingYear,
                    PercentageOrCGPA = model.PercentageOrCGPA,
                    DisplayOrder = resume.Educations.Count + 1
                };

                await _resumeService.AddEducationAsync(education);
                TempData["SuccessMessage"] = "Education entry added successfully!";
                return RedirectToAction(nameof(Educations), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "education";
            return View(nameof(Educations), resume.Educations.OrderBy(e => e.DisplayOrder).ToList());
        }

        // GET: /Resume/EditEducation/5
        [HttpGet]
        public async Task<IActionResult> EditEducation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var education = await _resumeService.GetEducationByIdAsync(id);
            if (education == null)
            {
                return NotFound();
            }

            var resume = await _resumeService.GetResumeByIdAsync(education.ResumeId);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            var model = new EducationViewModel
            {
                Id = education.Id,
                ResumeId = education.ResumeId,
                Degree = education.Degree,
                InstitutionName = education.InstitutionName,
                BoardOrUniversity = education.BoardOrUniversity,
                PassingYear = education.PassingYear,
                PercentageOrCGPA = education.PercentageOrCGPA,
                DisplayOrder = education.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "education";
            return View(model);
        }

        // POST: /Resume/EditEducation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEducation(int id, EducationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var education = await _resumeService.GetEducationByIdAsync(id);
            if (education == null)
            {
                return NotFound();
            }

            var resume = await _resumeService.GetResumeByIdAsync(education.ResumeId);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                education.Degree = model.Degree;
                education.InstitutionName = model.InstitutionName;
                education.BoardOrUniversity = model.BoardOrUniversity;
                education.PassingYear = model.PassingYear;
                education.PercentageOrCGPA = model.PercentageOrCGPA;

                await _resumeService.UpdateEducationAsync(education);
                TempData["SuccessMessage"] = "Education entry updated successfully!";
                return RedirectToAction(nameof(Educations), new { resumeId = education.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "education";
            return View(model);
        }

        // POST: /Resume/DeleteEducation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var education = await _resumeService.GetEducationByIdAsync(id);
            if (education == null)
            {
                return NotFound();
            }

            var resume = await _resumeService.GetResumeByIdAsync(education.ResumeId);
            if (resume == null || resume.UserId != user.Id)
            {
                return NotFound();
            }

            var resumeId = education.ResumeId;
            await _resumeService.DeleteEducationAsync(id);
            TempData["SuccessMessage"] = "Education entry deleted successfully!";
            return RedirectToAction(nameof(Educations), new { resumeId });
        }

        // GET: /Resume/Skills/5
        [HttpGet]
        public async Task<IActionResult> Skills(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "skills";

            return View(resume.Skills.OrderBy(s => s.DisplayOrder).ToList());
        }

        // POST: /Resume/AddSkill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSkill(SkillViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var skill = new Skill
                {
                    ResumeId = model.ResumeId,
                    SkillName = model.SkillName,
                    SkillCategory = model.SkillCategory,
                    Proficiency = model.Proficiency,
                    DisplayOrder = resume.Skills.Count + 1
                };

                await _resumeService.AddSkillAsync(skill);
                TempData["SuccessMessage"] = "Skill added successfully!";
                return RedirectToAction(nameof(Skills), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "skills";
            return View(nameof(Skills), resume.Skills.OrderBy(s => s.DisplayOrder).ToList());
        }

        // GET: /Resume/EditSkill/5
        [HttpGet]
        public async Task<IActionResult> EditSkill(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var skill = await _resumeService.GetSkillByIdAsync(id);
            if (skill == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(skill.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var model = new SkillViewModel
            {
                Id = skill.Id,
                ResumeId = skill.ResumeId,
                SkillName = skill.SkillName,
                SkillCategory = skill.SkillCategory,
                Proficiency = skill.Proficiency,
                DisplayOrder = skill.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "skills";
            return View(model);
        }

        // POST: /Resume/EditSkill/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(int id, SkillViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var skill = await _resumeService.GetSkillByIdAsync(id);
            if (skill == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(skill.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                skill.SkillName = model.SkillName;
                skill.SkillCategory = model.SkillCategory;
                skill.Proficiency = model.Proficiency;

                await _resumeService.UpdateSkillAsync(skill);
                TempData["SuccessMessage"] = "Skill updated successfully!";
                return RedirectToAction(nameof(Skills), new { resumeId = skill.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "skills";
            return View(model);
        }

        // POST: /Resume/DeleteSkill/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var skill = await _resumeService.GetSkillByIdAsync(id);
            if (skill == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(skill.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var resumeId = skill.ResumeId;
            await _resumeService.DeleteSkillAsync(id);
            TempData["SuccessMessage"] = "Skill deleted successfully!";
            return RedirectToAction(nameof(Skills), new { resumeId });
        }

        // GET: /Resume/Projects/5
        [HttpGet]
        public async Task<IActionResult> Projects(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "projects";

            return View(resume.Projects.OrderBy(p => p.DisplayOrder).ToList());
        }

        // POST: /Resume/AddProject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(ProjectViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var project = new Project
                {
                    ResumeId = model.ResumeId,
                    ProjectTitle = model.ProjectTitle,
                    Description = model.Description,
                    TechnologiesUsed = model.TechnologiesUsed,
                    GitHubLink = model.GitHubLink,
                    Duration = model.Duration,
                    DisplayOrder = resume.Projects.Count + 1
                };

                await _resumeService.AddProjectAsync(project);
                TempData["SuccessMessage"] = "Project added successfully!";
                return RedirectToAction(nameof(Projects), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "projects";
            return View(nameof(Projects), resume.Projects.OrderBy(p => p.DisplayOrder).ToList());
        }

        // GET: /Resume/EditProject/5
        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var project = await _resumeService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(project.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var model = new ProjectViewModel
            {
                Id = project.Id,
                ResumeId = project.ResumeId,
                ProjectTitle = project.ProjectTitle,
                Description = project.Description,
                TechnologiesUsed = project.TechnologiesUsed,
                GitHubLink = project.GitHubLink,
                Duration = project.Duration,
                DisplayOrder = project.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "projects";
            return View(model);
        }

        // POST: /Resume/EditProject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, ProjectViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var project = await _resumeService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(project.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                project.ProjectTitle = model.ProjectTitle;
                project.Description = model.Description;
                project.TechnologiesUsed = model.TechnologiesUsed;
                project.GitHubLink = model.GitHubLink;
                project.Duration = model.Duration;

                await _resumeService.UpdateProjectAsync(project);
                TempData["SuccessMessage"] = "Project updated successfully!";
                return RedirectToAction(nameof(Projects), new { resumeId = project.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "projects";
            return View(model);
        }

        // POST: /Resume/DeleteProject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var project = await _resumeService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(project.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var resumeId = project.ResumeId;
            await _resumeService.DeleteProjectAsync(id);
            TempData["SuccessMessage"] = "Project deleted successfully!";
            return RedirectToAction(nameof(Projects), new { resumeId });
        }

        // GET: /Resume/Experiences/5
        [HttpGet]
        public async Task<IActionResult> Experiences(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "experience";

            return View(resume.Experiences.OrderBy(e => e.DisplayOrder).ToList());
        }

        // POST: /Resume/AddExperience
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExperience(ExperienceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var experience = new Experience
                {
                    ResumeId = model.ResumeId,
                    CompanyName = model.CompanyName,
                    Position = model.Position,
                    StartDate = model.StartDate,
                    EndDate = model.IsCurrent ? null : model.EndDate,
                    IsCurrent = model.IsCurrent,
                    Responsibilities = model.Responsibilities,
                    DisplayOrder = resume.Experiences.Count + 1
                };

                await _resumeService.AddExperienceAsync(experience);
                TempData["SuccessMessage"] = "Work experience added successfully!";
                return RedirectToAction(nameof(Experiences), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "experience";
            return View(nameof(Experiences), resume.Experiences.OrderBy(e => e.DisplayOrder).ToList());
        }

        // GET: /Resume/EditExperience/5
        [HttpGet]
        public async Task<IActionResult> EditExperience(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var experience = await _resumeService.GetExperienceByIdAsync(id);
            if (experience == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(experience.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var model = new ExperienceViewModel
            {
                Id = experience.Id,
                ResumeId = experience.ResumeId,
                CompanyName = experience.CompanyName,
                Position = experience.Position,
                StartDate = experience.StartDate,
                EndDate = experience.EndDate,
                IsCurrent = experience.IsCurrent,
                Responsibilities = experience.Responsibilities,
                DisplayOrder = experience.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "experience";
            return View(model);
        }

        // POST: /Resume/EditExperience/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExperience(int id, ExperienceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var experience = await _resumeService.GetExperienceByIdAsync(id);
            if (experience == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(experience.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                experience.CompanyName = model.CompanyName;
                experience.Position = model.Position;
                experience.StartDate = model.StartDate;
                experience.EndDate = model.IsCurrent ? null : model.EndDate;
                experience.IsCurrent = model.IsCurrent;
                experience.Responsibilities = model.Responsibilities;

                await _resumeService.UpdateExperienceAsync(experience);
                TempData["SuccessMessage"] = "Work experience updated successfully!";
                return RedirectToAction(nameof(Experiences), new { resumeId = experience.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "experience";
            return View(model);
        }

        // POST: /Resume/DeleteExperience/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var experience = await _resumeService.GetExperienceByIdAsync(id);
            if (experience == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(experience.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var resumeId = experience.ResumeId;
            await _resumeService.DeleteExperienceAsync(id);
            TempData["SuccessMessage"] = "Work experience deleted successfully!";
            return RedirectToAction(nameof(Experiences), new { resumeId });
        }

        // GET: /Resume/Certifications/5
        [HttpGet]
        public async Task<IActionResult> Certifications(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "certifications";

            return View(resume.Certifications.OrderBy(c => c.DisplayOrder).ToList());
        }

        // POST: /Resume/AddCertification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCertification(CertificationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var certification = new Certification
                {
                    ResumeId = model.ResumeId,
                    CertificateName = model.CertificateName,
                    IssuingOrganization = model.IssuingOrganization,
                    IssueDate = model.IssueDate,
                    Description = model.Description,
                    CredentialUrl = model.CredentialUrl,
                    DisplayOrder = resume.Certifications.Count + 1
                };

                await _resumeService.AddCertificationAsync(certification);
                TempData["SuccessMessage"] = "Certification added successfully!";
                return RedirectToAction(nameof(Certifications), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "certifications";
            return View(nameof(Certifications), resume.Certifications.OrderBy(c => c.DisplayOrder).ToList());
        }

        // GET: /Resume/EditCertification/5
        [HttpGet]
        public async Task<IActionResult> EditCertification(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var certification = await _resumeService.GetCertificationByIdAsync(id);
            if (certification == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(certification.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var model = new CertificationViewModel
            {
                Id = certification.Id,
                ResumeId = certification.ResumeId,
                CertificateName = certification.CertificateName,
                IssuingOrganization = certification.IssuingOrganization,
                IssueDate = certification.IssueDate,
                Description = certification.Description,
                CredentialUrl = certification.CredentialUrl,
                DisplayOrder = certification.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "certifications";
            return View(model);
        }

        // POST: /Resume/EditCertification/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCertification(int id, CertificationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var certification = await _resumeService.GetCertificationByIdAsync(id);
            if (certification == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(certification.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                certification.CertificateName = model.CertificateName;
                certification.IssuingOrganization = model.IssuingOrganization;
                certification.IssueDate = model.IssueDate;
                certification.Description = model.Description;
                certification.CredentialUrl = model.CredentialUrl;

                await _resumeService.UpdateCertificationAsync(certification);
                TempData["SuccessMessage"] = "Certification updated successfully!";
                return RedirectToAction(nameof(Certifications), new { resumeId = certification.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "certifications";
            return View(model);
        }

        // POST: /Resume/DeleteCertification/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var certification = await _resumeService.GetCertificationByIdAsync(id);
            if (certification == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(certification.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var resumeId = certification.ResumeId;
            await _resumeService.DeleteCertificationAsync(id);
            TempData["SuccessMessage"] = "Certification deleted successfully!";
            return RedirectToAction(nameof(Certifications), new { resumeId });
        }

        // GET: /Resume/Achievements/5
        [HttpGet]
        public async Task<IActionResult> Achievements(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "achievements";

            return View(resume.Achievements.OrderBy(a => a.DisplayOrder).ToList());
        }

        // POST: /Resume/AddAchievement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAchievement(AchievementViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(model.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var achievement = new Achievement
                {
                    ResumeId = model.ResumeId,
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    DisplayOrder = resume.Achievements.Count + 1
                };

                await _resumeService.AddAchievementAsync(achievement);
                TempData["SuccessMessage"] = "Achievement added successfully!";
                return RedirectToAction(nameof(Achievements), new { resumeId = model.ResumeId });
            }

            ViewBag.ResumeId = model.ResumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "achievements";
            return View(nameof(Achievements), resume.Achievements.OrderBy(a => a.DisplayOrder).ToList());
        }

        // GET: /Resume/EditAchievement/5
        [HttpGet]
        public async Task<IActionResult> EditAchievement(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var achievement = await _resumeService.GetAchievementByIdAsync(id);
            if (achievement == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(achievement.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var model = new AchievementViewModel
            {
                Id = achievement.Id,
                ResumeId = achievement.ResumeId,
                Title = achievement.Title,
                Description = achievement.Description,
                Category = achievement.Category,
                DisplayOrder = achievement.DisplayOrder
            };

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "achievements";
            return View(model);
        }

        // POST: /Resume/EditAchievement/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAchievement(int id, AchievementViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var achievement = await _resumeService.GetAchievementByIdAsync(id);
            if (achievement == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(achievement.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                achievement.Title = model.Title;
                achievement.Description = model.Description;
                achievement.Category = model.Category;

                await _resumeService.UpdateAchievementAsync(achievement);
                TempData["SuccessMessage"] = "Achievement updated successfully!";
                return RedirectToAction(nameof(Achievements), new { resumeId = achievement.ResumeId });
            }

            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "achievements";
            return View(model);
        }

        // POST: /Resume/DeleteAchievement/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var achievement = await _resumeService.GetAchievementByIdAsync(id);
            if (achievement == null) return NotFound();

            var resume = await _resumeService.GetResumeByIdAsync(achievement.ResumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var resumeId = achievement.ResumeId;
            await _resumeService.DeleteAchievementAsync(id);
            TempData["SuccessMessage"] = "Achievement deleted successfully!";
            return RedirectToAction(nameof(Achievements), new { resumeId });
        }

        // GET: /Resume/Preview/5
        [HttpGet]
        public async Task<IActionResult> Preview(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            var templates = await _templateRepository.GetAllAsync();
            ViewBag.AvailableTemplates = templates.Where(t => t.IsActive).ToList();

            ViewBag.ResumeId = resumeId;
            ViewBag.ResumeTitle = resume.Title;
            ViewBag.ActiveTab = "preview";

            return View(resume);
        }

        // GET: /Resume/RenderHtml?resumeId=5&templateId=2
        [HttpGet]
        public async Task<IActionResult> RenderHtml(int resumeId, int? templateId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            if (templateId.HasValue)
            {
                resume.TemplateId = templateId.Value;
                resume.Template = null;
            }

            var htmlContent = _pdfService.BuildResumeHtml(resume);
            return Content(htmlContent, "text/html");
        }

        // POST: /Resume/UpdateTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTemplate(int resumeId, int templateId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            resume.TemplateId = templateId;
            await _resumeService.UpdateResumeAsync(resume);

            return Json(new { success = true });
        }

        // GET: /Resume/DownloadPdf?resumeId=5
        // This action generates a PDF from the resume data and streams it to the browser.
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(int resumeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var resume = await _resumeService.GetResumeByIdAsync(resumeId);
            if (resume == null || resume.UserId != user.Id) return NotFound();

            try
            {
                // Call the PDF service to generate the PDF bytes
                var pdfBytes = await _pdfService.GeneratePdfAsync(resume);

                // Build a clean filename from the resume title
                // e.g. "My SWE Resume" -> "My_SWE_Resume_Resume.pdf"
                var safeName = string.Concat(
                    (resume.FullName ?? resume.Title)
                    .Replace(" ", "_")
                    .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
                );
                var fileName = $"{safeName}_Resume.pdf";

                // Return the PDF as a file download
                // "application/pdf" tells the browser this is a PDF file
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Log the error and show a user-friendly error page
                TempData["ErrorMessage"] = $"PDF generation failed: {ex.Message}. Please try again.";
                return RedirectToAction(nameof(Preview), new { resumeId });
            }
        }
    }
}

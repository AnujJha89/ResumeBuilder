using ResumeBuilder.Core.Entities;
using System.Linq;

namespace ResumeBuilder.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            var allTemplates = GetAllTemplates();

            // Add any templates that don't already exist (by Name)
            var existingNames = context.ResumeTemplates.Select(t => t.Name).ToHashSet();
            var newTemplates = allTemplates.Where(t => !existingNames.Contains(t.Name)).ToArray();

            if (newTemplates.Length > 0)
            {
                context.ResumeTemplates.AddRange(newTemplates);
                context.SaveChanges();
            }
        }

        private static ResumeTemplate[] GetAllTemplates()
        {
            return new ResumeTemplate[]
            {
                // 1. Classic Minimalist (original)
                new ResumeTemplate
                {
                    Name = "Classic Minimalist",
                    ThumbnailPath = "/template_thumbnails/classic.png",
                    LaTeXCodeTemplate = @"\documentclass[10pt,letterpaper]{article}
\usepackage[utf8]{inputenc}
\usepackage[margin=0.75in]{geometry}
\usepackage{hyperref}
\usepackage{titlesec}
\usepackage{enumitem}

\titleformat{\section}{\large\bfseries}{}{0em}{}[\titlerule]
\titlespacing{\section}{0pt}{10pt}{5pt}

\begin{document}
\pagestyle{empty}

% Header
\begin{center}
    {\Huge \textbf{{{FullName}}}} \\
    \vspace{2pt}
    {{Phone}} $\cdot$ {{Email}} $\cdot$ {{Address}} \\
    \vspace{2pt}
    \href{{{LinkedInUrl}}}{LinkedIn} $\cdot$ \href{{{GitHubUrl}}}{GitHub}
\end{center}

\vspace{10pt}

% Summary
\section*{Professional Summary}
{{ProfessionalSummary}}

\vspace{10pt}

% Experience
\section*{Experience}
{{ExperienceList}}

\vspace{10pt}

% Education
\section*{Education}
{{EducationList}}

\vspace{10pt}

% Projects
\section*{Projects}
{{ProjectList}}

\vspace{10pt}

% Skills
\section*{Technical Skills}
{{SkillList}}

\vspace{10pt}

% Certifications
\section*{Certifications}
{{CertificationList}}

\end{document}",
                    IsActive = true
                },

                // 2. Modern Professional
                new ResumeTemplate
                {
                    Name = "Modern Professional",
                    ThumbnailPath = "/template_thumbnails/modern_professional.png",
                    LaTeXCodeTemplate = @"\documentclass[11pt,a4paper]{article}
\usepackage[utf8]{inputenc}
\usepackage[left=0.6in,right=0.6in,top=0.5in,bottom=0.5in]{geometry}
\usepackage{hyperref}
\usepackage{titlesec}
\usepackage{enumitem}
\usepackage{xcolor}
\usepackage{fontawesome5}
\usepackage{graphicx}
\usepackage{tikz}

\definecolor{primary}{HTML}{2D3748}
\definecolor{accent}{HTML}{3182CE}

\hypersetup{colorlinks=true,urlcolor=accent}

\titleformat{\section}{\color{primary}\large\bfseries\uppercase}{}{0em}{}[\color{accent}\titlerule]
\titlespacing{\section}{0pt}{12pt}{6pt}

\begin{document}
\pagestyle{empty}

% Header
{\centering
    \ifx\empty{{ProfilePicturePath}}\empty
    \else
        \begin{tikzpicture}
            \clip (0,0) circle (1.25cm);
            \node at (0,0) {\includegraphics[width=2.5cm]{{{ProfilePicturePath}}}};
        \end{tikzpicture} \\[8pt]
    \fi
    {\fontsize{28}{34}\selectfont\bfseries\color{primary} {{FullName}}} \\[4pt]
    {\large\color{accent} {{JobTitle}}} \\[8pt]
    \faPhone\ {{Phone}} \quad \faEnvelope\ {{Email}} \quad \faMapMarker*\ {{Address}} \\[3pt]
    \faLinkedin\ \href{{{LinkedInUrl}}}{LinkedIn} \quad \faGithub\ \href{{{GitHubUrl}}}{GitHub} \\
\par}

\vspace{14pt}

% Summary
\section{Profile}
{{ProfessionalSummary}}

% Experience
\section{Work Experience}
{{ExperienceList}}

% Education
\section{Education}
{{EducationList}}

% Projects
\section{Projects}
{{ProjectList}}

% Skills
\section{Technical Skills}
{{SkillList}}

% Certifications
\section{Certifications}
{{CertificationList}}

\end{document}",
                    IsActive = true
                },

                // 3. Executive Elegant
                new ResumeTemplate
                {
                    Name = "Executive Elegant",
                    ThumbnailPath = "/template_thumbnails/executive_elegant.png",
                    LaTeXCodeTemplate = @"\documentclass[11pt,letterpaper]{article}
\usepackage[utf8]{inputenc}
\usepackage[margin=0.7in]{geometry}
\usepackage{hyperref}
\usepackage{titlesec}
\usepackage{enumitem}
\usepackage{xcolor}
\usepackage{fancyhdr}

\definecolor{darkblue}{HTML}{1A365D}
\definecolor{gold}{HTML}{C59D5F}

\hypersetup{colorlinks=true,urlcolor=darkblue}

\titleformat{\section}{\color{darkblue}\Large\scshape}{}{0em}{}[\color{gold}\rule{\textwidth}{1.5pt}]
\titlespacing{\section}{0pt}{14pt}{8pt}

\pagestyle{fancy}
\fancyhf{}
\renewcommand{\headrulewidth}{0pt}
\fancyfoot[C]{\footnotesize\color{darkblue} {{FullName}} \quad$\cdot$\quad Page \thepage}

\begin{document}

% Header
\begin{center}
    {\fontsize{30}{36}\selectfont\scshape\color{darkblue} {{FullName}}} \\[6pt]
    {\large\color{gold}\bfseries {{JobTitle}}} \\[10pt]
    {{Phone}} \enspace$|$\enspace {{Email}} \enspace$|$\enspace {{Address}} \\[3pt]
    \href{{{LinkedInUrl}}}{LinkedIn} \enspace$|$\enspace \href{{{GitHubUrl}}}{GitHub}
\end{center}

\vspace{16pt}

\section{Executive Summary}
{{ProfessionalSummary}}

\section{Professional Experience}
{{ExperienceList}}

\section{Education}
{{EducationList}}

\section{Key Projects}
{{ProjectList}}

\section{Core Competencies}
{{SkillList}}

\section{Certifications \& Awards}
{{CertificationList}}

\end{document}",
                    IsActive = true
                },

                // 4. Tech Developer
                new ResumeTemplate
                {
                    Name = "Tech Developer",
                    ThumbnailPath = "/template_thumbnails/tech_developer.png",
                    LaTeXCodeTemplate = @"\documentclass[10pt,a4paper]{article}
\usepackage[utf8]{inputenc}
\usepackage[left=0.5in,right=0.5in,top=0.4in,bottom=0.4in]{geometry}
\usepackage{hyperref}
\usepackage{titlesec}
\usepackage{enumitem}
\usepackage{xcolor}
\usepackage{tabularx}

\definecolor{headerbg}{HTML}{1A202C}
\definecolor{accentgreen}{HTML}{38A169}
\definecolor{lightgray}{HTML}{718096}

\hypersetup{colorlinks=true,urlcolor=accentgreen}

\titleformat{\section}{\bfseries\large\color{headerbg}}{}{0em}{}
\titlespacing{\section}{0pt}{10pt}{4pt}

\setlist[itemize]{leftmargin=*, nosep, label={\color{accentgreen}\textbullet}}

\begin{document}
\pagestyle{empty}

% Header
\noindent
\begin{tabularx}{\textwidth}{@{}X r@{}}
    {\fontsize{24}{28}\selectfont\bfseries {{FullName}}} & {\color{lightgray}\small {{Phone}}} \\
    {\color{accentgreen}\large {{JobTitle}}} & {\color{lightgray}\small {{Email}}} \\
    & {\color{lightgray}\small {{Address}}} \\
    & {\small \href{{{LinkedInUrl}}}{LinkedIn} $\cdot$ \href{{{GitHubUrl}}}{GitHub}}
\end{tabularx}

\vspace{4pt}
\noindent\rule{\textwidth}{2pt}
\vspace{8pt}

\section{Summary}
{{ProfessionalSummary}}

\section{Experience}
{{ExperienceList}}

\section{Projects}
{{ProjectList}}

\section{Technical Skills}
{{SkillList}}

\section{Education}
{{EducationList}}

\section{Certifications}
{{CertificationList}}

\end{document}",
                    IsActive = true
                },

                // 5. Creative Portfolio
                new ResumeTemplate
                {
                    Name = "Creative Portfolio",
                    ThumbnailPath = "/template_thumbnails/creative_portfolio.png",
                    LaTeXCodeTemplate = @"",
                    IsActive = true
                },

                // 6. Academic CV
                new ResumeTemplate
                {
                    Name = "Academic CV",
                    ThumbnailPath = "/template_thumbnails/academic_cv.png",
                    LaTeXCodeTemplate = @"",
                    IsActive = true
                },

                // 7. Stylish Minimal
                new ResumeTemplate
                {
                    Name = "Stylish Minimal",
                    ThumbnailPath = "/template_thumbnails/stylish_minimal.png",
                    LaTeXCodeTemplate = @"",
                    IsActive = true
                }
            };
        }
    }
}

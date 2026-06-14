using ResumeBuilder.Core.Entities;

namespace ResumeBuilder.Core.Interfaces
{
    /// <summary>
    /// Contract for PDF generation from a Resume entity.
    /// The controller depends on this abstraction, not on any concrete PDF library.
    /// This follows the Dependency Inversion Principle (DIP) from SOLID.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Generates a PDF from the given fully-loaded resume entity.
        /// Returns the raw PDF bytes ready to stream to the browser.
        /// </summary>
        /// <param name="resume">The resume with all child collections populated.</param>
        /// <returns>PDF file as a byte array.</returns>
        Task<byte[]> GeneratePdfAsync(Resume resume);

        /// <summary>
        /// Generates the raw HTML content of the resume, styled according to its template.
        /// </summary>
        /// <param name="resume">The resume with all child collections populated.</param>
        /// <returns>A string of raw HTML.</returns>
        string BuildResumeHtml(Resume resume);
    }
}

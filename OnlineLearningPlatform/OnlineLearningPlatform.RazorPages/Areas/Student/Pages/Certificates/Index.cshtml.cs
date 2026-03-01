using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Certificate;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Certificates
{
    public class IndexModel : PageModel
    {
        private readonly ICertificateService _certService;
        public List<CertificateDto> Certificates { get; set; } = new();

        public IndexModel(ICertificateService certService)
        {
            _certService = certService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            Certificates = await _certService.GetMyCertificatesAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnGetDownloadAsync(string code)
        {
            try 
            {
                var pdfBytes = await _certService.GeneratePdfAsync(code);
                var cert = await _certService.GetByCodeAsync(code);
                var fileName = $"Certificate_{cert?.CourseTitle.Replace(" ", "_") ?? code.Trim()}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch(Exception)
            {
                return RedirectToPage("/Student/Certificates/Index");
            }
        }

        public async Task<IActionResult> OnGetViewAsync(string code)
        {
            try
            {
                var pdfBytes = await _certService.GeneratePdfAsync(code);
                // content-disposition inline explicitly
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception)
            {
                return RedirectToPage("/Student/Certificates/Index");
            }
        }
    }
}

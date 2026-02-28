using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel(
        ISectionService sectionService,
        ICourseService courseService,
        ILessonService lessonService,
        IWebHostEnvironment environment) : PageModel
    {
        private const long MaxVideoSizeBytes = 200L * 1024 * 1024;
        private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];

        [BindProperty(SupportsGet = true)]
        public int SectionId { get; set; }

        public Guid CourseId { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public LessonType LessonType { get; set; } = LessonType.Reading;

        [BindProperty]
        public int OrderIndex { get; set; } = 1;

        [BindProperty]
        public bool IsPreview { get; set; }

        [BindProperty]
        public string? LessonContent { get; set; }

        [BindProperty]
        public IFormFile? VideoFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var section = await sectionService.GetByIdAsync(SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found. You must create/select a valid section first.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission for this section.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            CourseId = section.CourseId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var section = await sectionService.GetByIdAsync(SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found. You must create/select a valid section first.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission for this section.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            CourseId = section.CourseId;

            if (LessonType == LessonType.Video)
            {
                if (VideoFile == null || VideoFile.Length == 0)
                {
                    ModelState.AddModelError(nameof(VideoFile), "Video file is required when LessonType is Video.");
                }
                else
                {
                    if (VideoFile.Length > MaxVideoSizeBytes)
                    {
                        ModelState.AddModelError(nameof(VideoFile), "Video file size must be less than or equal to 200MB.");
                    }

                    var extension = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                    if (!AllowedVideoExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(nameof(VideoFile), "Invalid video format. Allowed: .mp4, .webm, .ogg.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string? videoStoragePath = null;
            string? originalFileName = null;

            if (LessonType == LessonType.Video && VideoFile != null)
            {
                var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadRoot);

                var extension = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                var generatedFileName = $"{Guid.NewGuid():N}{extension}";
                var absolutePath = Path.Combine(uploadRoot, generatedFileName);

                await using (var stream = System.IO.File.Create(absolutePath))
                {
                    await VideoFile.CopyToAsync(stream);
                }

                videoStoragePath = $"/uploads/videos/{generatedFileName}";
                originalFileName = VideoFile.FileName;
            }

            var lesson = new Lesson
            {
                SectionId = SectionId,
                Title = Title,
                LessonType = LessonType,
                OrderIndex = OrderIndex <= 0 ? 1 : OrderIndex,
                IsPreview = IsPreview,
                Content = LessonContent,
                VideoStoragePath = videoStoragePath,
                VideoOriginalFileName = originalFileName,
                VideoStatus = LessonType == LessonType.Video ? VideoStatus.Ready : null
            };

            var result = await lessonService.CreateAsync(lesson, teacherId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = section.CourseId });
        }
    }
}
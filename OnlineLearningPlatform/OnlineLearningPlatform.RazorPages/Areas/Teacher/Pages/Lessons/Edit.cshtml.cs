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
    public class EditModel(
        ILessonService lessonService,
        ISectionService sectionService,
        ICourseService courseService,
        IWebHostEnvironment environment) : PageModel
    {
        private const long MaxVideoSizeBytes = 200L * 1024 * 1024;
        private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];
        private const long MaxPdfSizeBytes = 50L * 1024 * 1024;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
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

        [BindProperty]
        public IFormFile? ReadingPdfFile { get; set; }

        public string? CurrentVideoFileName { get; set; }
        public string? CurrentReadingPdfFileName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var lesson = await lessonService.GetByIdAsync(Id);
            if (lesson == null)
            {
                TempData["ErrorMessage"] = "Lesson not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var section = await sectionService.GetByIdAsync(lesson.SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this lesson.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            CourseId = section.CourseId;
            SectionId = lesson.SectionId;
            Title = lesson.Title;
            LessonType = lesson.LessonType;
            OrderIndex = lesson.OrderIndex;
            IsPreview = lesson.IsPreview;
            LessonContent = lesson.Content;
            CurrentVideoFileName = lesson.VideoOriginalFileName;
            CurrentReadingPdfFileName = lesson.ReadingPdfOriginalFileName;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var existing = await lessonService.GetByIdAsync(Id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Lesson not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var section = await sectionService.GetByIdAsync(existing.SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this lesson.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            SectionId = existing.SectionId;
            CourseId = section.CourseId;
            CurrentVideoFileName = existing.VideoOriginalFileName;
            CurrentReadingPdfFileName = existing.ReadingPdfOriginalFileName;

            if (LessonType == LessonType.Video && VideoFile != null && VideoFile.Length > 0)
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

            if (LessonType == LessonType.Reading && ReadingPdfFile != null && ReadingPdfFile.Length > 0)
            {
                var extension = Path.GetExtension(ReadingPdfFile.FileName).ToLowerInvariant();
                if (extension != ".pdf")
                {
                    ModelState.AddModelError(nameof(ReadingPdfFile), "Only PDF file is allowed for Reading attachment.");
                }

                if (ReadingPdfFile.Length > MaxPdfSizeBytes)
                {
                    ModelState.AddModelError(nameof(ReadingPdfFile), "PDF size must be less than or equal to 50MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string? videoStoragePath = existing.VideoStoragePath;
            string? videoOriginalFileName = existing.VideoOriginalFileName;

            if (LessonType == LessonType.Video && VideoFile != null && VideoFile.Length > 0)
            {
                var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadRoot);

                var extension = Path.GetExtension(VideoFile.FileName).ToLowerInvariant();
                var generatedFileName = $"{Guid.NewGuid():N}{extension}";
                var absolutePath = Path.Combine(uploadRoot, generatedFileName);

                await using var stream = System.IO.File.Create(absolutePath);
                await VideoFile.CopyToAsync(stream);

                videoStoragePath = $"/uploads/videos/{generatedFileName}";
                videoOriginalFileName = VideoFile.FileName;
            }

            if (LessonType != LessonType.Video)
            {
                videoStoragePath = null;
                videoOriginalFileName = null;
            }

            string? readingPdfStoragePath = existing.ReadingPdfStoragePath;
            string? readingPdfOriginalFileName = existing.ReadingPdfOriginalFileName;

            if (LessonType == LessonType.Reading && ReadingPdfFile != null && ReadingPdfFile.Length > 0)
            {
                var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "readings");
                Directory.CreateDirectory(uploadRoot);

                var generatedFileName = $"{Guid.NewGuid():N}.pdf";
                var absolutePath = Path.Combine(uploadRoot, generatedFileName);

                await using var stream = System.IO.File.Create(absolutePath);
                await ReadingPdfFile.CopyToAsync(stream);

                readingPdfStoragePath = $"/uploads/readings/{generatedFileName}";
                readingPdfOriginalFileName = ReadingPdfFile.FileName;
            }

            if (LessonType != LessonType.Reading)
            {
                readingPdfStoragePath = null;
                readingPdfOriginalFileName = null;
            }

            var lesson = new Lesson
            {
                LessonId = Id,
                SectionId = existing.SectionId,
                Title = Title,
                LessonType = LessonType,
                OrderIndex = OrderIndex <= 0 ? 1 : OrderIndex,
                IsPreview = IsPreview,
                Content = LessonContent,
                VideoStoragePath = videoStoragePath,
                VideoOriginalFileName = videoOriginalFileName,
                VideoStatus = LessonType == LessonType.Video ? VideoStatus.Ready : null,
                ReadingPdfStoragePath = readingPdfStoragePath,
                ReadingPdfOriginalFileName = readingPdfOriginalFileName
            };

            var result = await lessonService.UpdateAsync(lesson, teacherId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = CourseId });
        }
    }
}

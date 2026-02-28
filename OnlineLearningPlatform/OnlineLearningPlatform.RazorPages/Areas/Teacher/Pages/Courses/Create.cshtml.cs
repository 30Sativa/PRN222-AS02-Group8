using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Course.Request;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;
        private readonly ISectionService _sectionService;
        private readonly ILessonService _lessonService;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _dbContext;

        private const long MaxVideoSizeBytes = 200L * 1024 * 1024;
        private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];
        private const long MaxPdfSizeBytes = 50L * 1024 * 1024;

        public CreateModel(
            ICourseService courseService,
            ICategoryService categoryService,
            ISectionService sectionService,
            ILessonService lessonService,
            IWebHostEnvironment environment,
            ApplicationDbContext dbContext)
        {
            _courseService = courseService;
            _categoryService = categoryService;
            _sectionService = sectionService;
            _lessonService = lessonService;
            _environment = environment;
            _dbContext = dbContext;
        }

        [BindProperty]
        public CourseUpsertRequest Input { get; set; } = new();

        [BindProperty]
        public List<SectionInputModel> Sections { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllAsync();
            EnsureAtLeastOneSection();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _categoryService.GetAllAsync();
            EnsureAtLeastOneSection();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            ValidateDynamicStructureAndFiles();
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await _courseService.CreateAsync(teacherId, Input);
                if (!result.Success || result.Course == null)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return Page();
                }

                foreach (var sectionInput in Sections.OrderBy(s => NormalizeOrder(s.OrderIndex)))
                {
                    var sectionResult = await _sectionService.CreateAsync(
                        result.Course.CourseId,
                        sectionInput.Title,
                        NormalizeOrder(sectionInput.OrderIndex),
                        teacherId);

                    if (!sectionResult.Success || sectionResult.Section == null)
                    {
                        ModelState.AddModelError(string.Empty, $"Create section '{sectionInput.Title}' failed: {sectionResult.Message}");
                        return Page();
                    }

                    foreach (var lessonInput in sectionInput.Lessons.OrderBy(l => NormalizeOrder(l.OrderIndex)))
                    {
                        var lesson = await BuildLessonEntityAsync(sectionResult.Section.SectionId, lessonInput);
                        var lessonResult = await _lessonService.CreateAsync(lesson, teacherId);
                        if (!lessonResult.Success)
                        {
                            ModelState.AddModelError(string.Empty, $"Create lesson '{lessonInput.Title}' failed: {lessonResult.Message}");
                            return Page();
                        }
                    }
                }

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Course, sections, and lessons created successfully.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                return Page();
            }
        }

        private async Task<Lesson> BuildLessonEntityAsync(int sectionId, LessonInputModel lessonInput)
        {
            string? videoStoragePath = null;
            string? videoOriginalFileName = null;
            string? readingPdfStoragePath = null;
            string? readingPdfOriginalFileName = null;

            if (lessonInput.LessonType == LessonType.Video && lessonInput.VideoFile != null && lessonInput.VideoFile.Length > 0)
            {
                var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "videos");
                Directory.CreateDirectory(uploadRoot);

                var extension = Path.GetExtension(lessonInput.VideoFile.FileName).ToLowerInvariant();
                var generatedFileName = $"{Guid.NewGuid():N}{extension}";
                var absolutePath = Path.Combine(uploadRoot, generatedFileName);

                await using (var stream = System.IO.File.Create(absolutePath))
                {
                    await lessonInput.VideoFile.CopyToAsync(stream);
                }

                videoStoragePath = $"/uploads/videos/{generatedFileName}";
                videoOriginalFileName = lessonInput.VideoFile.FileName;
            }

            if (lessonInput.LessonType == LessonType.Reading && lessonInput.ReadingPdfFile != null && lessonInput.ReadingPdfFile.Length > 0)
            {
                var uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "readings");
                Directory.CreateDirectory(uploadRoot);

                var generatedFileName = $"{Guid.NewGuid():N}.pdf";
                var absolutePath = Path.Combine(uploadRoot, generatedFileName);

                await using (var stream = System.IO.File.Create(absolutePath))
                {
                    await lessonInput.ReadingPdfFile.CopyToAsync(stream);
                }

                readingPdfStoragePath = $"/uploads/readings/{generatedFileName}";
                readingPdfOriginalFileName = lessonInput.ReadingPdfFile.FileName;
            }

            return new Lesson
            {
                SectionId = sectionId,
                Title = lessonInput.Title,
                LessonType = lessonInput.LessonType,
                OrderIndex = NormalizeOrder(lessonInput.OrderIndex),
                IsPreview = lessonInput.IsPreview,
                Content = lessonInput.LessonType == LessonType.Reading ? lessonInput.Content : null,
                VideoStoragePath = videoStoragePath,
                VideoOriginalFileName = videoOriginalFileName,
                VideoStatus = lessonInput.LessonType == LessonType.Video ? VideoStatus.Ready : null,
                ReadingPdfStoragePath = readingPdfStoragePath,
                ReadingPdfOriginalFileName = readingPdfOriginalFileName
            };
        }

        private void ValidateDynamicStructureAndFiles()
        {
            if (Sections == null || Sections.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "At least one section is required.");
                return;
            }

            var duplicatedSectionOrder = Sections
                .GroupBy(s => NormalizeOrder(s.OrderIndex))
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatedSectionOrder != null)
            {
                ModelState.AddModelError(string.Empty, $"Section order {duplicatedSectionOrder.Key} is duplicated.");
            }

            for (var i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];

                if (string.IsNullOrWhiteSpace(section.Title))
                {
                    ModelState.AddModelError($"Sections[{i}].Title", "Section title is required.");
                }

                if (section.Lessons == null || section.Lessons.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, $"Section '{section.Title}' must have at least one lesson.");
                    continue;
                }

                var duplicatedLessonOrder = section.Lessons
                    .GroupBy(l => NormalizeOrder(l.OrderIndex))
                    .FirstOrDefault(g => g.Count() > 1);

                if (duplicatedLessonOrder != null)
                {
                    ModelState.AddModelError(string.Empty, $"Section '{section.Title}' has duplicated lesson order {duplicatedLessonOrder.Key}.");
                }

                for (var j = 0; j < section.Lessons.Count; j++)
                {
                    var lesson = section.Lessons[j];

                    if (string.IsNullOrWhiteSpace(lesson.Title))
                    {
                        ModelState.AddModelError($"Sections[{i}].Lessons[{j}].Title", "Lesson title is required.");
                    }

                    ValidateLessonFileRule(i, j, lesson);
                }
            }
        }

        private void ValidateLessonFileRule(int sectionIndex, int lessonIndex, LessonInputModel lesson)
        {
            var keyPrefix = $"Sections[{sectionIndex}].Lessons[{lessonIndex}]";

            if (lesson.LessonType == LessonType.Video)
            {
                if (lesson.VideoFile == null || lesson.VideoFile.Length == 0)
                {
                    ModelState.AddModelError($"{keyPrefix}.VideoFile", "Video file is required when LessonType is Video.");
                    return;
                }

                if (lesson.VideoFile.Length > MaxVideoSizeBytes)
                {
                    ModelState.AddModelError($"{keyPrefix}.VideoFile", "Video file size must be less than or equal to 200MB.");
                }

                var extension = Path.GetExtension(lesson.VideoFile.FileName).ToLowerInvariant();
                if (!AllowedVideoExtensions.Contains(extension))
                {
                    ModelState.AddModelError($"{keyPrefix}.VideoFile", "Invalid video format. Allowed: .mp4, .webm, .ogg.");
                }
            }
            else if (lesson.LessonType == LessonType.Reading && lesson.ReadingPdfFile != null && lesson.ReadingPdfFile.Length > 0)
            {
                var extension = Path.GetExtension(lesson.ReadingPdfFile.FileName).ToLowerInvariant();
                if (extension != ".pdf")
                {
                    ModelState.AddModelError($"{keyPrefix}.ReadingPdfFile", "Only PDF file is allowed for Reading attachment.");
                }

                if (lesson.ReadingPdfFile.Length > MaxPdfSizeBytes)
                {
                    ModelState.AddModelError($"{keyPrefix}.ReadingPdfFile", "PDF size must be less than or equal to 50MB.");
                }
            }
        }

        private static int NormalizeOrder(int order) => order <= 0 ? 1 : order;

        private void EnsureAtLeastOneSection()
        {
            if (Sections == null)
            {
                Sections = new List<SectionInputModel>();
            }

            if (Sections.Count == 0)
            {
                Sections.Add(new SectionInputModel
                {
                    Title = "Section 1",
                    OrderIndex = 1,
                    Lessons = new List<LessonInputModel>
                    {
                        new LessonInputModel
                        {
                            Title = "Lesson 1",
                            OrderIndex = 1,
                            LessonType = LessonType.Reading
                        }
                    }
                });
            }
        }

        public class SectionInputModel
        {
            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            public int OrderIndex { get; set; } = 1;

            public List<LessonInputModel> Lessons { get; set; } = new();
        }

        public class LessonInputModel
        {
            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            public LessonType LessonType { get; set; } = LessonType.Reading;

            public int OrderIndex { get; set; } = 1;

            public bool IsPreview { get; set; }

            public string? Content { get; set; }

            public IFormFile? VideoFile { get; set; }

            public IFormFile? ReadingPdfFile { get; set; }
        }
    }
}

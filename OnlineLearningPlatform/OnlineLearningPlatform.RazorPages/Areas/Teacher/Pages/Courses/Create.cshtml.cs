using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        private const long MaxVideoSizeBytes = 200L * 1024 * 1024;
        private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".ogg"];
        private const long MaxPdfSizeBytes = 50L * 1024 * 1024;

        public CreateModel(
            ICourseService courseService,
            ICategoryService categoryService,
            ISectionService sectionService,
            ILessonService lessonService,
            IWebHostEnvironment environment)
        {
            _courseService = courseService;
            _categoryService = categoryService;
            _sectionService = sectionService;
            _lessonService = lessonService;
            _environment = environment;
        }

        [BindProperty]
        public CourseUpsertRequest Input { get; set; } = new();

        [BindProperty]
        public List<SectionInputModel> Sections { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllAsync();
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

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _categoryService.GetAllAsync();

            if (!ModelState.IsValid)
            {
                EnsureAtLeastOneSection();
                return Page();
            }

            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            ValidateDynamicStructure();
            if (!ModelState.IsValid)
            {
                EnsureAtLeastOneSection();
                return Page();
            }

            var result = await _courseService.CreateAsync(teacherId, Input);
            if (!result.Success || result.Course == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                EnsureAtLeastOneSection();
                return Page();
            }

            foreach (var sectionInput in Sections.OrderBy(s => s.OrderIndex))
            {
                var sectionResult = await _sectionService.CreateAsync(
                    result.Course.CourseId,
                    sectionInput.Title,
                    sectionInput.OrderIndex <= 0 ? 1 : sectionInput.OrderIndex,
                    teacherId);

                if (!sectionResult.Success || sectionResult.Section == null)
                {
                    ModelState.AddModelError(string.Empty, $"Course created, but failed to create section '{sectionInput.Title}': {sectionResult.Message}");
                    return Page();
                }

                foreach (var lessonInput in sectionInput.Lessons.OrderBy(l => l.OrderIndex))
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

                    var lesson = new Lesson
                    {
                        SectionId = sectionResult.Section.SectionId,
                        Title = lessonInput.Title,
                        LessonType = lessonInput.LessonType,
                        OrderIndex = lessonInput.OrderIndex <= 0 ? 1 : lessonInput.OrderIndex,
                        IsPreview = lessonInput.IsPreview,
                        Content = lessonInput.LessonType == LessonType.Reading ? lessonInput.Content : null,
                        VideoStoragePath = videoStoragePath,
                        VideoOriginalFileName = videoOriginalFileName,
                        VideoStatus = lessonInput.LessonType == LessonType.Video ? VideoStatus.Ready : null,
                        ReadingPdfStoragePath = readingPdfStoragePath,
                        ReadingPdfOriginalFileName = readingPdfOriginalFileName
                    };

                    var lessonResult = await _lessonService.CreateAsync(lesson, teacherId);
                    if (!lessonResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, $"Course created, but failed to create lesson '{lessonInput.Title}': {lessonResult.Message}");
                        return Page();
                    }
                }
            }

            TempData["SuccessMessage"] = "Course, sections, and lessons created successfully.";
            return RedirectToPage("/Courses/Index", new { area = "Teacher" });
        }

        private void ValidateDynamicStructure()
        {
            if (Sections == null || Sections.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "At least one section is required.");
                return;
            }

            var duplicatedSectionOrder = Sections
                .GroupBy(s => s.OrderIndex <= 0 ? 1 : s.OrderIndex)
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
                    .GroupBy(l => l.OrderIndex <= 0 ? 1 : l.OrderIndex)
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

                    if (lesson.LessonType == LessonType.Video)
                    {
                        if (lesson.VideoFile == null || lesson.VideoFile.Length == 0)
                        {
                            ModelState.AddModelError($"Sections[{i}].Lessons[{j}].VideoFile", "Video file is required when lesson type is Video.");
                        }
                        else
                        {
                            if (lesson.VideoFile.Length > MaxVideoSizeBytes)
                            {
                                ModelState.AddModelError($"Sections[{i}].Lessons[{j}].VideoFile", "Video file size must be less than or equal to 200MB.");
                            }

                            var ext = Path.GetExtension(lesson.VideoFile.FileName).ToLowerInvariant();
                            if (!AllowedVideoExtensions.Contains(ext))
                            {
                                ModelState.AddModelError($"Sections[{i}].Lessons[{j}].VideoFile", "Invalid video format. Allowed: .mp4, .webm, .ogg.");
                            }
                        }
                    }

                    if (lesson.LessonType == LessonType.Reading && lesson.ReadingPdfFile != null && lesson.ReadingPdfFile.Length > 0)
                    {
                        var ext = Path.GetExtension(lesson.ReadingPdfFile.FileName).ToLowerInvariant();
                        if (ext != ".pdf")
                        {
                            ModelState.AddModelError($"Sections[{i}].Lessons[{j}].ReadingPdfFile", "Only PDF file is allowed for Reading attachment.");
                        }

                        if (lesson.ReadingPdfFile.Length > MaxPdfSizeBytes)
                        {
                            ModelState.AddModelError($"Sections[{i}].Lessons[{j}].ReadingPdfFile", "PDF size must be less than or equal to 50MB.");
                        }
                    }
                }
            }
        }

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

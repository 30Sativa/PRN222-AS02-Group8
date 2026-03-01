using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.EntityFrameworkCore;

using OnlineLearningPlatform.Models;

using OnlineLearningPlatform.Models.Entities;

using OnlineLearningPlatform.Services.Interface;

using System.Security.Claims;



namespace OnlineLearningPlatform.RazorPages.Pages.Lessons

{

    [Authorize(Roles = "Teacher,Student")]

    public class DetailModel(ILessonService lessonService, ApplicationDbContext dbContext) : PageModel

    {

        public Lesson? Lesson { get; private set; }



        public int WatchedSeconds { get; private set; }



        public async Task<IActionResult> OnGetAsync(int id)

        {

            Lesson = await lessonService.GetByIdAsync(id);

            if (Lesson == null)

            {

                TempData["ErrorMessage"] = "Lesson not found.";

                return RedirectToPage("/Auth/Index");

            }



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))

            {

                WatchedSeconds = await dbContext.LessonProgresses

                    .Where(x => x.UserId == userId && x.LessonId == id)

                    .Select(x => x.WatchedSeconds)

                    .FirstOrDefaultAsync();

            }



            return Page();

        }



        public async Task<IActionResult> OnPostSaveProgressAsync(int id, [FromBody] SaveProgressRequest request)

        {

            if (request == null || request.WatchedSeconds < 0)

            {

                return BadRequest(new { success = false, message = "Invalid payload." });

            }



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))

            {

                return new JsonResult(new { success = false, message = "Unauthorized." }) { StatusCode = 401 };

            }



            var lesson = await lessonService.GetByIdAsync(id);

            if (lesson == null || lesson.LessonType != LessonType.Video)

            {

                return NotFound(new { success = false, message = "Video lesson not found." });

            }



            var watchedSeconds = request.WatchedSeconds;

            if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)

            {

                watchedSeconds = Math.Min(watchedSeconds, lesson.VideoDurationSeconds.Value);

            }



            var progress = await dbContext.LessonProgresses

                .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == id);



            if (progress == null)

            {

                progress = new LessonProgress

                {

                    ProgressId = Guid.NewGuid(),

                    UserId = userId,

                    LessonId = id,

                    WatchedSeconds = watchedSeconds,

                    IsCompleted = false

                };



                dbContext.LessonProgresses.Add(progress);

            }

            else

            {

                progress.WatchedSeconds = Math.Max(progress.WatchedSeconds, watchedSeconds);

            }



            if (lesson.VideoDurationSeconds.HasValue && lesson.VideoDurationSeconds.Value > 0)

            {

                var completeThreshold = Math.Max(1, lesson.VideoDurationSeconds.Value - 5);

                if (progress.WatchedSeconds >= completeThreshold)

                {

                    progress.IsCompleted = true;

                    progress.CompletedAt ??= DateTime.UtcNow;

                }

            }



            await dbContext.SaveChangesAsync();



            return new JsonResult(new { success = true, watchedSeconds = progress.WatchedSeconds, isCompleted = progress.IsCompleted });

        }



        public class SaveProgressRequest

        {

            public int WatchedSeconds { get; set; }

        }

    }

}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.DTOs.Review;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Courses
{
    public class DetailsModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IReviewService _reviewService;

        public DetailsModel(IStudentService studentService, IReviewService reviewService)
        {
            _studentService = studentService;
            _reviewService = reviewService;
        }

        public StudentCourseResponse Course { get; set; } = null!;
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public ReviewDto? MyReview { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        [BindProperty]
        public int ReviewRating { get; set; }

        [BindProperty]
        public string ReviewComment { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _studentService.GetCourseDetailAsync(id, userId);

            if (course == null)
            {
                return NotFound();
            }

            Course = course;

            var stats = await _reviewService.GetCourseRatingStatsAsync(id);
            AverageRating = stats.average;
            ReviewCount = stats.count;

            Reviews = await _reviewService.GetCourseReviewsAsync(id, 1, 50);

            if (!string.IsNullOrEmpty(userId) && course.IsEnrolled)
            {
                MyReview = await _reviewService.GetUserReviewAsync(userId, id);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitReviewAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (ReviewRating < 1 || ReviewRating > 5)
            {
                ReviewRating = 5;
            }

            await _reviewService.SubmitReviewAsync(userId, id, ReviewRating, ReviewComment);
            return RedirectToPage(new { id = id });
        }
    }
}

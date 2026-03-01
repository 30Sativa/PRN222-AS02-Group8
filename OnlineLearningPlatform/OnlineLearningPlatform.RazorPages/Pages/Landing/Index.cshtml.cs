using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Review;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Landing
{
    public class IndexModel : PageModel
    {
        private readonly IStudentService _studentService;
        private readonly IReviewService _reviewService;

        public IndexModel(IStudentService studentService, IReviewService reviewService)
        {
            _studentService = studentService;
            _reviewService = reviewService;
        }

        public List<StudentCourseResponse> Courses { get; set; } = new();
        public Dictionary<Guid, List<ReviewDto>> CourseReviews { get; set; } = new();
        public Dictionary<Guid, (double Average, int Count)> CourseRatings { get; set; } = new();

        public string? SearchKeyword { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;
        private const int PageSize = 12;

        public async Task OnGetAsync(string? keyword = null, int page = 1)
        {
            SearchKeyword = keyword;
            CurrentPage = page < 1 ? 1 : page;

            // Load courses without user context (guest view)
            var result = await _studentService.SearchCoursesAsync(keyword, null, CurrentPage, PageSize);
            Courses = result.Courses;
            TotalCount = result.TotalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Load reviews and ratings for each course
            foreach (var course in Courses)
            {
                var reviews = await _reviewService.GetCourseReviewsAsync(course.CourseId, 1, 3);
                var ratings = await _reviewService.GetCourseRatingStatsAsync(course.CourseId);

                if (reviews.Any())
                {
                    CourseReviews[course.CourseId] = reviews;
                    CourseRatings[course.CourseId] = ratings;
                }
            }
        }
    }
}

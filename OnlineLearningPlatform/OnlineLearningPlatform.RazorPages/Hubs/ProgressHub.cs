using Microsoft.AspNetCore.SignalR;

namespace OnlineLearningPlatform.RazorPages.Hubs
{
    /// <summary>
    /// SignalR Hub cho real-time progress update.
    /// Client join group theo courseId để nhận update khi có người hoàn thành bài học.
    /// </summary>
    public class ProgressHub : Hub
    {
        /// <summary>
        /// Student join group theo CourseId khi vào trang LearnCourse.
        /// </summary>
        public async Task JoinCourseGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"course_{courseId}");
        }

        /// <summary>
        /// Student rời group khi rời trang.
        /// </summary>
        public async Task LeaveCourseGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course_{courseId}");
        }
    }
}

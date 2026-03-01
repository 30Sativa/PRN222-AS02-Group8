using Microsoft.AspNetCore.SignalR;

namespace OnlineLearningPlatform.RazorPages.Hubs
{
    /// <summary>
    /// Hub trung tâm cho real-time CRUD events.
    /// Server chỉ push (broadcast), không cần xử lý client call.
    /// Dùng IHubContext&lt;DataHub&gt; trong PageModel để gửi event sau khi CRUD thành công.
    /// </summary>
    public class DataHub : Hub
    {
        // Hub này là server-push only.
        // Tất cả method broadcast được gọi từ IHubContext<DataHub> inject vào PageModel.
    }
}

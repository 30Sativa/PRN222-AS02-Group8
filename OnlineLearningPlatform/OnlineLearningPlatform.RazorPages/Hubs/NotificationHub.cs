using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OnlineLearningPlatform.RazorPages.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Hub này được dùng chung để push event Notification tới User 
        // SignalR tự động map ConnectionId với UserIdentifier based on Claims
    }
}


using ChillChaser.Models.DB;
using System.Security.Claims;

namespace ChillChaser.Services.impl
{
    public class NotificationService(IAppService _appService) : INotificationService
    {
        public async Task CreateNotification(CCDbContext ctx, string appName, string title, string content, DateTime receivedAt, string userId)
        {
            var app = await _appService.CreateOrGetApp(ctx, appName);
            app.Notifications.Add(new Notification
            {
                Title = title,
                Content = content,
                ReceivedAt = receivedAt,
                UserId = userId
            });
        }
    }
}

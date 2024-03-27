namespace ChillChaser.Services
{
    public interface INotificationService
    {
        Task CreateNotification(CCDbContext ctx, string appName, string title, string content, DateTime receivedAt, string userId);
    }
}

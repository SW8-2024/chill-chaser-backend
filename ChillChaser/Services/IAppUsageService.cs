using ChillChaser.Models.Request;

namespace ChillChaser.Services
{
    public interface IAppUsageService
    {
        Task AddAppUsage(CCDbContext ctx, DateTime from, DateTime to, AppSession[] sessions, string appName, string userId);
    }
}

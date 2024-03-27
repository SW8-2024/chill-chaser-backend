namespace ChillChaser.Services
{
    public interface IAppUsageService
    {
        Task AddAppUsage(CCDbContext ctx, string appName, DateTime from, DateTime to, string userId);
    }
}

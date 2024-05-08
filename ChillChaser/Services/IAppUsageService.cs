using ChillChaser.Models.Request;

namespace ChillChaser.Services
{
    public interface IAppUsageService
    {
        Task AddAppUsage(CCDbContext ctx, IEnumerable<CreateAppUsage> sessions, string userId);
    }
}

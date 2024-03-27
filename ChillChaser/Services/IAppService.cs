using ChillChaser.Models.DB;

namespace ChillChaser.Services
{
    public interface IAppService
    {
        Task<App> CreateOrGetApp(CCDbContext ctx, string appName);
    }
}

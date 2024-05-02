using ChillChaser.Models.DB;

namespace ChillChaser.Services
{
    public interface IAppService
    {
        Task<App> CreateOrGetApp(CCDbContext ctx, string appName);
		Task<IDictionary<string, App>> CreateOrGetApps(CCDbContext ctx, ISet<string> appNames);
	}
}

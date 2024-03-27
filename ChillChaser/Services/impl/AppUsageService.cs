
using ChillChaser.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace ChillChaser.Services.impl
{
    public class AppUsageService(IAppService appService) : IAppUsageService
    {
        public async Task AddAppUsage(CCDbContext ctx, string appName, DateTime from, DateTime to, string userId)
        {
            var app = await appService.CreateOrGetApp(ctx, appName);

            app.AppUsage.Add(new AppUsage
            {
                From = from,
                To = to,
                UserId = userId
            });
        }
    }
}

using ChillChaser.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace ChillChaser.Services.impl
{
    public class AppService : IAppService
    {
        public async Task<App> CreateOrGetApp(CCDbContext ctx, string appName)
        {
            var app = await ctx.Apps.SingleOrDefaultAsync(a => a.Name == appName);

            if (app == null)
            {
                app = new App { Name = appName };
                await ctx.Apps.AddAsync(app);
            }

            return app;
        }
    }
}

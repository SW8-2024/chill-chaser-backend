
using System.ComponentModel.DataAnnotations;
using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ChillChaser.Services.impl
{
    public class AppUsageService(IAppService appService) : IAppUsageService
    {
        public async Task AddAppUsage(CCDbContext ctx, IEnumerable<CreateAppUsage> appUsages, string userId)
        {
            foreach (var usage in appUsages)
            {
                var app = await appService.CreateOrGetApp(ctx, usage.AppName);
                var existingSessionsForApp = (from appUsage in ctx.AppUsages
                    where appUsage.UserId == userId && appUsage.App == app
                    select appUsage);

                foreach (var session in usage.Sessions)
                {
                    var sessionWithSameTime = (from appUsage in existingSessionsForApp
                        where appUsage.From == session.From || appUsage.To == session.To
                        select appUsage);
                    if (!sessionWithSameTime.Any())
                    {
                        app.AppUsage.Add(new AppUsage
                        {
                            From = session.From,
                            To = session.To,
                            UserId = userId,
                            App = app
                        });
                    }
                    else
                    {
                        foreach (var usageToUpdate in sessionWithSameTime)
                        {
                            usageToUpdate.To = usage.To;
                        }
                    }
                }
            }
            await ctx.SaveChangesAsync();
        }
    }
}

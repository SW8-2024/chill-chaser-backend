
using System.ComponentModel.DataAnnotations;
using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ChillChaser.Services.impl
{
    public class AppUsageService(IAppService appService) : IAppUsageService
    {
        public async Task AddAppUsage(CCDbContext ctx, DateTime fromVar, DateTime to, AppSession[] sessions, string appName, string userId)
        {
            var app = await appService.CreateOrGetApp(ctx, appName);
            foreach (var elem in sessions){
                var existingSessions = (from appUsage in ctx.AppUsages
                    where appUsage.UserId == userId && appUsage.App == app && (appUsage.From == elem.From || appUsage.To == elem.To)     
                    select appUsage).ToList();
                
                if (!existingSessions.Any()){
                    app.AppUsage.Add(new AppUsage
                        {
                            From = elem.From,
                            To = elem.To,
                            UserId = userId,
                            App = app
                    });
                }
                else
                {
                    foreach (var session in existingSessions)
                    {
                        session.To = elem.To;
                    }
                }         
            }
            
        }
    }
}

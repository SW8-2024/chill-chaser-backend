
using ChillChaser.Models.DB;
using System.Security.Claims;

namespace ChillChaser.Services.impl
{
    public class HeartRateService() : IHeartRateService
    {
        public async Task AddHeartRate(CCDbContext ctx, int bpm, DateTime dateTime, string userId)
        {
            await ctx.HeartRates.AddAsync(new Models.DB.HeartRate
            {
                Bpm = bpm,
                DateTime = dateTime,
                UserId = userId,
            });
        }
    }
}


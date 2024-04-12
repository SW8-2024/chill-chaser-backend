
using ChillChaser.Models.DB;
using System.Security.Claims;

namespace ChillChaser.Services.impl
{
    public class HeartRateService() : IHeartRateService
    {
        public async Task AddHeartRate(CCDbContext ctx, int bpm, DateTime dateTime, string userId)
        {
            ctx.HeartRates.Add(new Models.DB.HeartRate
            {
                Bpm = bpm,
                DateTime = dateTime,
                UserId = userId,
            });
        }
    }
}


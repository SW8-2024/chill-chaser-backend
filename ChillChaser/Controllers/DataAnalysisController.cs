using ChillChaser.Models.Response;
using ChillChaser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChillChaser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataAnalysisController(IBreakDownService breakDownService, CCDbContext ctx) : ControllerBase
    {
        [Authorize]
        [HttpGet("breakdown", Name = "BreakDown")]
        [ProducesResponseType(typeof(GetBreakdownDataResponse), 200)]
        public async Task<IActionResult> BreakDown(DateTime date)
        {
            if (date < new DateTime(2020, 1, 1))
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No user id");

            var analysisRange = breakDownService.GetLastMonthRange(date);

            var stressByApp = await breakDownService.GetStressByApp(ctx, userId, analysisRange);
            var dailyStress = await breakDownService.GetDailyStress(ctx, userId, analysisRange);
            var stressMetrics = await breakDownService.GetStressMetrics(ctx, userId, analysisRange);

            return Ok(new GetBreakdownDataResponse
            {
                AverageStress = stressMetrics.Average,
                DailyStressDataPoints = dailyStress,
                StressByApp = stressByApp
            });
        }

        [Authorize]
        [HttpGet("stress-metrics", Name = "StressMetrics")]
        [ProducesResponseType(typeof(GetStressMetricsResponse), 200)]
        public async Task<IActionResult> StressMetrics(DateTime date)
        {
            if (date < new DateTime(2020, 1, 1))
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No user id");

            var todayRange = breakDownService.GetTodayRange(date);
            var metrics = await breakDownService.GetStressMetrics(ctx, userId, todayRange);
            var latestHeartRate = await breakDownService.GetLatestHeartRate(ctx, userId);
            return Ok(new GetStressMetricsResponse
            {
                Min = metrics.Min,
                Average = metrics.Average,
                Max = metrics.Max,
                Latest = latestHeartRate
            });
        }

        [Authorize]
        [HttpGet("app-breakdown", Name = "AppBreakDown")]
        [ProducesResponseType(typeof(AppUsageAnalysisResponse), 200)]
        public async Task<IActionResult> AppBreakDown()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No user id");

            var appUsageAnalysis = await breakDownService.GetAppUsageAnalysis(ctx, userId);

            return Ok(new AppUsageAnalysisResponse {
                AppUsageAnalysis = appUsageAnalysis
            });
        }
    }
}

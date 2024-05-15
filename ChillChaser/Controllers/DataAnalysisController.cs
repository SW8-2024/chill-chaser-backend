using ChillChaser.Models.Response;
using ChillChaser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChillChaser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataAnalysisController(IAnalysisService analysisService, CCDbContext ctx) : ControllerBase
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

            var analysisRange = analysisService.GetLastMonthRange(date);

            var stressByApp = await analysisService.GetStressByApp(ctx, userId, analysisRange);
            var dailyStress = await analysisService.GetDailyStress(ctx, userId, analysisRange);
            var stressMetrics = await analysisService.GetStressMetrics(ctx, userId, analysisRange);

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

            var todayRange = analysisService.GetTodayRange(date);
            var metrics = await analysisService.GetStressMetrics(ctx, userId, todayRange);
            var latestHeartRate = await analysisService.GetLatestHeartRate(ctx, userId);
            return Ok(new GetStressMetricsResponse
            {
                Min = metrics.Min,
                Average = metrics.Average,
                Max = metrics.Max,
                Latest = latestHeartRate.Value,
                LatestDateTime = latestHeartRate.DateTime
            });
        }

        [Authorize]
        [HttpGet("app-breakdown", Name = "AppBreakDown")]
        [ProducesResponseType(typeof(AppUsageAnalysisResponse), 200)]
        public async Task<IActionResult> AppBreakDown()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No user id");

            var appUsageAnalysis = await analysisService.GetAppUsageAnalysis(ctx, userId);

            return Ok(new AppUsageAnalysisResponse {
                AppUsageAnalysis = appUsageAnalysis
            });
        }

        [Authorize]
        [HttpGet("per-app-per-day-usage-analysis", Name = "PerAppPerDayUsageAnalysis")]
        [ProducesResponseType(typeof(PerAppPerDayUsageResponse), 200)]
        public async Task<IActionResult> PerAppPerDayUsageAnalysis(DateTime endOfDay, string appName) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("No user id");
            
            return Ok(new PerAppPerDayUsageResponse {
                AppUsageForAppAndDays = await analysisService.GetAppPerDayUsageAnalysis(ctx, userId, endOfDay, appName)
            });
        }

        [Authorize]
        [HttpGet("analysis-for-day-and-app", Name = "AnalysisForDayAndApp")]
        [ProducesResponseType(typeof(AnalysisForDayAndAppResponse), 200)]
        public async Task<IActionResult> AnalysisForDayAndApp(DateTime endOfDay, string appName) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");

            return Ok(new AnalysisForDayAndAppResponse {
                AppUsageAnalysis = await analysisService.GetAppUsageForDay(ctx, userId, endOfDay, appName),
                HighResolutionStress = await analysisService.GetHighResolutionStressForDayAndApp(ctx, userId, appName, endOfDay)
            });
        }
    }
}

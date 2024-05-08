using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using ChillChaser.Models.Response;
using ChillChaser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ChillChaser.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class DataCollectionController(
		CCDbContext ctx,
		IAppUsageService appUsageService,
		INotificationService notificationService,
		IHeartRateService heartRateService,
		IBreakDownService breakDownService
	) : ControllerBase {
		private readonly CCDbContext _ctx = ctx;
		private readonly IAppUsageService _appUsageService = appUsageService;
		private readonly INotificationService _notificationService = notificationService;
		private readonly IHeartRateService _heartRateService = heartRateService;
		private readonly IBreakDownService _breakDownService = breakDownService;

		[Authorize]
		[HttpPost("notification", Name = "CreateNotification")]
		public async Task<IActionResult> CreateNotification(CreateNotification model) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? throw new Exception("No user id");

			await _notificationService.CreateNotification(ctx, model.AppName, model.Title, model.Content, model.ReceivedAt, userId);

			await _ctx.SaveChangesAsync();

			return Ok();
		}

		[Authorize]
		[HttpGet("notification", Name = "GetNotifications")]
		[ProducesResponseType(typeof(GetNotificationResponse), 200)]
		public async Task<IActionResult> GetNotifications() {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? throw new Exception("No user id");

			var notificationsMapped = from notification in _ctx.Notifications
									  where notification.UserId == userId
									  select new GetNotificationResponse {
										  Id = notification.Id,
										  Title = notification.Title,
										  Content = notification.Content,
										  ReceivedAt = notification.ReceivedAt,
										  AppName = notification.SourceApp.Name,
									  };

			return Ok(await notificationsMapped.ToListAsync());
		}

        [Authorize]
        [HttpPost("app-usage", Name = "CreateAppUsage")]
        public async Task<IActionResult> CreateAppUsage(IEnumerable<CreateAppUsage> model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");
            await _appUsageService.AddAppUsage(_ctx, model, userId);
            await _ctx.SaveChangesAsync();
            return Ok();

			await _ctx.SaveChangesAsync();
			return Ok();
		}

		[Authorize]
		[HttpGet("app-usage", Name = "GetAppUsage")]
		[ProducesResponseType(typeof(GetAppUsageResponse), 200)]
		public async Task<IActionResult> GetAppUsage() {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? throw new Exception("No user id");

			var appUsageMapped = from appUsage in _ctx.AppUsages
								 where appUsage.UserId == userId
								 select new GetAppUsageResponse {
									 Id = appUsage.Id,
									 AppName = appUsage.App.Name,
									 From = appUsage.From,
									 To = appUsage.To,
								 };

			return Ok(await appUsageMapped.ToListAsync());
		}

		[Authorize]
		[HttpPost("heartRate", Name = "CreateHeartRate")]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateHeartRate(CreateHeartRate model) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
						 ?? throw new Exception("No user id");
			await _heartRateService.AddHeartRate(_ctx, model.Bpm, model.DateTime, userId);

			await _ctx.SaveChangesAsync();
			return Ok();
		}

		[Authorize]
		[HttpGet("heartRate", Name = "GetHeartRate")]
		[ProducesResponseType(typeof(GetHeartRateResponse), 200)]
		public async Task<IActionResult> GetHeartRate(DateTime? dateFrom, DateTime? to) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
						 ?? throw new Exception("No user id");

			var heartRateMapped = from heartRate in _ctx.HeartRates
								  where heartRate.UserId == userId
								  && heartRate.DateTime < to && heartRate.DateTime > dateFrom
								  select new HeartRateResponse() {
									  Id = heartRate.Id,
									  Bpm = heartRate.Bpm,
									  DateTime = heartRate.DateTime,
									  UserId = heartRate.UserId
								  };

			return Ok(await heartRateMapped.ToListAsync());
		}


		[HttpGet("leak", Name = "Leak")]
		[ProducesResponseType(typeof(GetAppUsageResponse), 200)]
		public async Task<IActionResult> Leak() {
			var appUsageMapped = from appUsage in _ctx.AppUsages
								 select new GetAppUsageResponse {
									 Id = appUsage.Id,
									 AppName = appUsage.App.Name,
									 From = appUsage.From,
									 To = appUsage.To,
								 };

			return Ok(await appUsageMapped.ToListAsync());
		}

		[Authorize]
		[HttpGet("breakdown", Name = "BreakDown")]
		[ProducesResponseType(typeof(GetBreakdownDataResponse), 200)]
		public async Task<IActionResult> BreakDown(DateTime date) {
			if (date < new DateTime(2020, 1, 1)) {
				return BadRequest();
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new Exception("No user id");

			var analysisRange = _breakDownService.GetLastMonthRange(date);

			var stressByApp = await _breakDownService.GetStressByApp(_ctx, userId, analysisRange);
			var dailyStress = await _breakDownService.GetDailyStress(_ctx, userId, analysisRange);
			var stressMetrics = await _breakDownService.GetStressMetrics(_ctx, userId, analysisRange);

			return Ok(new GetBreakdownDataResponse {
				AverageStress = stressMetrics.Average,
				DailyStressDataPoints = dailyStress,
				StressByApp = stressByApp
			});
		}

		[Authorize]
		[HttpGet("stress-metrics", Name = "StressMetrics")]
		[ProducesResponseType(typeof(GetStressMetricsResponse), 200)]
		public async Task<IActionResult> StressMetrics(DateTime date) {
			if (date < new DateTime(2020, 1, 1)) {
				return BadRequest();
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new Exception("No user id");

			var todayRange = _breakDownService.GetTodayRange(date);
			var metrics = await _breakDownService.GetStressMetrics(ctx, userId, todayRange);
			var latestHeartRate = await _breakDownService.GetLatestHeartRate(ctx, userId);
			return Ok(new GetStressMetricsResponse {
				Min = metrics.Min,
				Average = metrics.Average,
				Max = metrics.Max,
				Latest = latestHeartRate
			});
		}

	}
}

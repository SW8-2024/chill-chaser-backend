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
	public class DataCollectionController(CCDbContext ctx, IAppUsageService appUsageService, INotificationService notificationService, IHeartRateService heartRateService) : ControllerBase {
		private readonly CCDbContext _ctx = ctx;
		private readonly IAppUsageService _appUsageService = appUsageService;
		private readonly INotificationService _notificationService = notificationService;
		private readonly IHeartRateService _heartRateService = heartRateService;

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
		public async Task<IActionResult> CreateAppUsage(CreateAppUsage model) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
					?? throw new Exception("No user id");
			await _appUsageService.AddAppUsage(_ctx, model.AppName, model.From, model.To, userId);

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

		private void PadDates(List<DailyStressDataPoint> stressPoints, DateOnly from, DateOnly to) {
			var index = 0;
			for (DateOnly current = from; current < to; current = current.AddDays(1)) {
				var currentObj = new DailyStressDataPoint {
					Date = current,
					Value = 0
				};

				if (index >= stressPoints.Count) {
					stressPoints.Add(currentObj);
					index++;
				}
				else if (current < stressPoints[index].Date) {
					stressPoints.Insert(index, currentObj);
					index++;
				}
				else if (current >= stressPoints[index].Date) {
					index++;
				}
			}
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

			var endTime = new DateOnly(date.Year, date.Month, date.Day);
			var startTime = endTime.AddMonths(-1);

			var stressByApp = await _ctx.Database.SqlQuery<StressByAppDataPoint>($"""
				SELECT 
					a."Name" as "Name", AVG(hr."Bpm") AS "Value"
				FROM "AppUsages" au
					JOIN "HeartRates" hr ON hr."DateTime" >= au."From" AND hr."DateTime" <= au."To"
					JOIN "Apps" a ON a."Id" = au."AppId"
				WHERE au."From" >= {startTime} AND au."To" <= {endTime} 
					AND au."UserId" = {userId} AND hr."UserId" = {userId}
				GROUP BY au."AppId", a."Name"
			""").ToListAsync();

			var dailyStress = await _ctx.Database.SqlQuery<DailyStressDataPoint>($"""
				SELECT 
					hr."DateTime"::DATE AS "Date", AVG(hr."Bpm") AS "Value"
				FROM "HeartRates" hr
				WHERE hr."DateTime" > {startTime} AND hr."DateTime" < {endTime}
					AND hr."UserId" = {userId}
				GROUP BY hr."DateTime"::DATE
				ORDER BY "Date" ASC
			""").ToListAsync();

			var averageStress = await _ctx.Database.SqlQuery<double?>($"""
				SELECT AVG(hr."Bpm") AS "Value"
				FROM "HeartRates" hr
				WHERE hr."DateTime" > {startTime} AND hr."DateTime" < {endTime}
					AND hr."UserId" = {userId}
			""").SingleOrDefaultAsync();
			PadDates(dailyStress, startTime, endTime);

			return Ok(new GetBreakdownDataResponse {
				AverageStress = averageStress ?? -1,
				DailyStressDataPoints = dailyStress,
				StressByApp = stressByApp
			});
		}
	}
}

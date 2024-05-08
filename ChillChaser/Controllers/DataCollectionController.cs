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
using System.Collections.Generic;
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
		IAnalysisService breakDownService
	) : ControllerBase {
		private readonly CCDbContext _ctx = ctx;
		private readonly IAppUsageService _appUsageService = appUsageService;
		private readonly INotificationService _notificationService = notificationService;
		private readonly IHeartRateService _heartRateService = heartRateService;
		private readonly IAnalysisService _breakDownService = breakDownService;

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
		[HttpPost("add-test-data", Name = "AddTestData")]
		public async Task AddTestData() {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new Exception("No user id");

			var end = DateTime.UtcNow;
			var start = end.AddDays(-5);


			double currentBpm = 80;
			Random rnd = new Random();

			for (DateTime current = start; current < end; current = current.AddSeconds(4 + rnd.NextDouble()))
			{
				_ctx.HeartRates.Add(new HeartRate
				{
					Bpm = (int) currentBpm,
					DateTime = current,
					UserId = userId
				});

				currentBpm += (rnd.NextDouble() - 0.5);

			}

			List<List<AppSession>> appUsages = new()
			{
				new List<AppSession>(),
				new List<AppSession>(),
				new List<AppSession>(),
				new List<AppSession>()
			};
			DateTime sessionBegin = start;

			for (DateTime current = start; current < end; current = current.AddMinutes(1 + rnd.NextDouble() * 3))
			{
				var decision = rnd.Next(0, 100);
				if (decision < appUsages.Count) {
					appUsages[decision].Add(new AppSession
					{
						From = sessionBegin,
						To = current
					});
					sessionBegin = current;
				} else if (decision < 7)
				{
					sessionBegin = current;
				}
			}
			await _ctx.SaveChangesAsync();
			await _appUsageService.AddAppUsage(_ctx, new List<CreateAppUsage>()
			{
				new CreateAppUsage
				{
					AppName = "app 1",
					Sessions = appUsages[0]
				},
				new CreateAppUsage
				{
					AppName = "app 2",
					Sessions = appUsages[1]
				},
				new CreateAppUsage
				{
					AppName = "app 3",
					Sessions = appUsages[2]
				},
				new CreateAppUsage
				{
					AppName = "app 4",
					Sessions = appUsages[3]
				},
			}, userId);
		}

		[Authorize]
		[HttpPost("clear-data", Name = "ClearData")]
		public async Task ClearData()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? throw new Exception("No user id");
			await _ctx.HeartRates.Where(hr => hr.UserId == userId).ExecuteDeleteAsync();
			await _ctx.AppUsages.Where(hr => hr.UserId == userId).ExecuteDeleteAsync();
		}
	}
}

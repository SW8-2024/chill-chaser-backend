using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using ChillChaser.Models.Response;
using ChillChaser.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ChillChaser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataCollectionController(CCDbContext ctx, IAppUsageService appUsageService, INotificationService notificationService) : ControllerBase
    {
        private readonly CCDbContext _ctx = ctx;
        private readonly IAppUsageService _appUsageService = appUsageService;
        private readonly INotificationService _notificationService = notificationService;

        [Authorize]
        [HttpPost("notification", Name = "CreateNotification")]
        public async Task<IActionResult> CreateNotification(CreateNotification model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");

            await _notificationService.CreateNotification(ctx, model.AppName, model.Title, model.Content, model.ReceivedAt, userId);

            await _ctx.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("notification", Name = "GetNotifications")]
        [ProducesResponseType(typeof(GetNotificationResponse), 200)]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");

            var notificationsMapped = from notification in _ctx.Notifications
                                      where notification.UserId == userId
                                      select new GetNotificationResponse
                                      {
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
        public async Task<IActionResult> CreateAppUsage(CreateAppUsage model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");
            await _appUsageService.AddAppUsage(_ctx, model.AppName, model.From, model.To, userId);
            
            await _ctx.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("app-usage", Name = "GetAppUsage")]
        [ProducesResponseType(typeof(GetAppUsageResponse), 200)]
        public async Task<IActionResult> GetAppUsage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new Exception("No user id");

            var appUsageMapped = from appUsage in _ctx.AppUsages
                                 where appUsage.UserId == userId
                                 select new GetAppUsageResponse
                                 {
                                     Id = appUsage.Id,
                                     AppName = appUsage.App.Name,
                                     From = appUsage.From,
                                     To = appUsage.To,
                                 };

            return Ok(await appUsageMapped.ToListAsync());
        }

        [HttpGet("leak", Name = "Leak")]
        [ProducesResponseType(typeof(GetAppUsageResponse), 200)]
        public async Task<IActionResult> Leak()
        {
            var appUsageMapped = from appUsage in _ctx.AppUsages
                                 select new GetAppUsageResponse
                                 {
                                     Id = appUsage.Id,
                                     AppName = appUsage.App.Name,
                                     From = appUsage.From,
                                     To = appUsage.To,
                                 };

            return Ok(await appUsageMapped.ToListAsync());
        }
    }
}

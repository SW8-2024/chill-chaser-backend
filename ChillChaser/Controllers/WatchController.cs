using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using ChillChaser.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChillChaser.Controllers {
	[Route("api/[controller]")]
	[ApiController]
	public class WatchController(
		IWatchAuthService watchAuthService, 
		SignInManager<CCUser> signInManager,
		UserManager<CCUser> userManager,
		CCDbContext dbContext
	) : ControllerBase {
		private readonly IWatchAuthService _watchAuthService = watchAuthService;
		private readonly SignInManager<CCUser> _signInManager = signInManager;
		private readonly UserManager<CCUser> _userManager = userManager;
		private readonly CCDbContext _ctx = dbContext;

		[HttpPost("login", Name = "WatchLogin")]
		public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> StartWatchAuthSession(StartAuthSessionRequest body) {
			var userId = await _watchAuthService.WaitForAuth(body.Token);
			var user = await _ctx.Users.SingleAsync(u => u.Id == userId);
			_signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
			await _signInManager.SignInAsync(user, true);
			return TypedResults.Empty;
		}

		[Authorize]
		[HttpPost("authorize", Name = "WatchAuthorize")]
		public async Task<IActionResult> AuthWatch(AuthorizeWatchRequest body) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
						?? throw new Exception("No user id");
			await _watchAuthService.GrantAccess(body.Token, userId);
			return Ok();
		}
	}
}

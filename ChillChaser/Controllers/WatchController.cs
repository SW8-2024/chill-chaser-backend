using ChillChaser.Models.DB;
using ChillChaser.Models.Exceptions;
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
		CCDbContext dbContext
	) : ControllerBase {
		private readonly IWatchAuthService _watchAuthService = watchAuthService;
		private readonly SignInManager<CCUser> _signInManager = signInManager;
		private readonly CCDbContext _ctx = dbContext;

		[HttpPost("login", Name = "WatchLogin")]
		public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, BadRequest<string>, UnauthorizedHttpResult>> StartWatchAuthSession(StartAuthSessionRequest body) {
			try {
				var userId = await _watchAuthService.WaitForAuth(body.Token);
				var user = await _ctx.Users.SingleAsync(u => u.Id == userId);
				_signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
				await _signInManager.SignInAsync(user, true);
			} catch (WatchAuthSessionTimeoutException) {
				return TypedResults.Unauthorized();
			} catch (TokenAlreadyUsedException) {
				return TypedResults.BadRequest("TokenAlreadyUsed");
			}
			return TypedResults.Empty;
		}

		[Authorize]
		[HttpPost("authorize", Name = "WatchAuthorize")]
		public async Task<Results<Ok, BadRequest<string>>> AuthWatch(AuthorizeWatchRequest body) {
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
						?? throw new Exception("No user id");
			try {
				await _watchAuthService.GrantAccess(body.Token, userId);
			} catch (UnknownTokenException) {
				return TypedResults.BadRequest("UnknownToken");
			}
			return TypedResults.Ok();
		}
	}
}

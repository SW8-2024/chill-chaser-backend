using ChillChaser.Models.DB;
using ChillChaser.Models.Exceptions;
using ChillChaser.Models.Internal;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;

namespace ChillChaser.Services.impl {
	public class WatchAuthService() : IWatchAuthService {
		private readonly Dictionary<string, WatchAuthSession> _authSessions = new();
		public async Task GrantAccess(string sessionToken, string userId) {
			if (_authSessions.TryGetValue(sessionToken, out var was))
				was.TaskCompletionSource.SetResult(userId);
			_authSessions.Remove(sessionToken);
		}

		public async Task<string> WaitForAuth(string sessionToken) {
			var taskCompletionSource = new TaskCompletionSource<string>();
			try {
				_authSessions.Add(sessionToken, new WatchAuthSession {
					Token = sessionToken,
					Expires = DateTime.Now.AddMinutes(10),
					TaskCompletionSource = taskCompletionSource
				});
				return await taskCompletionSource.Task;
			} catch (ArgumentException) {
				throw new TokenAlreadyUsedException("Given token is already used");
			} catch {
				_authSessions.Remove(sessionToken);
				throw;
			}
		}
	}
}

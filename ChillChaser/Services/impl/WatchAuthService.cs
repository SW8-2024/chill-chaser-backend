using ChillChaser.Models.DB;
using ChillChaser.Models.Exceptions;
using ChillChaser.Models.Internal;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;

namespace ChillChaser.Services.impl {
	public class WatchAuthService() : IWatchAuthService {
		private readonly ConcurrentDictionary<string, WatchAuthSession> _authSessions = new();
		public async Task GrantAccess(string sessionToken, string userId) {
			if (_authSessions.TryGetValue(sessionToken, out var was))
				was.TaskCompletionSource.SetResult(userId);
			else
				throw new UnknownTokenException("Token was invalid");
			_authSessions.TryRemove(sessionToken, out _);
		}

		public async Task<string> WaitForAuth(string sessionToken) {
			var taskCompletionSource = new TaskCompletionSource<string>();
			var tokenExpiresMs = 10 * 60 * 1000;
			try
			{
				var wasAdded = _authSessions.TryAdd(sessionToken, new WatchAuthSession
				{
					Token = sessionToken,
					Expires = DateTime.Now.AddMilliseconds(tokenExpiresMs),
					TaskCompletionSource = taskCompletionSource
				});

				if (!wasAdded)
				{
					throw new TokenAlreadyUsedException("Given token is already used");
				}

				var task = await Task.WhenAny(
					Task.Run(async () => { return (string?)await taskCompletionSource.Task; }),
					Task.Run(async () => { await Task.Delay(tokenExpiresMs); return (string?)null; })
				);
				var value = await task;
				_authSessions.TryRemove(sessionToken, out _);
				if (value == null)
				{
					taskCompletionSource.SetCanceled();
					throw new WatchAuthSessionTimeoutException("The auth session expired");
				}
				else
				{
					return value;
				}
			}
			catch (TokenAlreadyUsedException)
			{
				throw;
			}
			catch
			{
				_authSessions.TryRemove(sessionToken, out _);
				throw;
			}
		}
	}
}

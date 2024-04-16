using Microsoft.AspNetCore.Authentication.BearerToken;

namespace ChillChaser.Models.Internal {
	public class WatchAuthSession {
		public required string Token { get; set; }
		public required DateTime Expires { get; set; }
		public required TaskCompletionSource<string> TaskCompletionSource { get; set; }
	}
}

namespace ChillChaser.Models.Exceptions {
	public class WatchAuthSessionTimeoutException : Exception {
		public WatchAuthSessionTimeoutException(string? message) : base(message) {
		}
	}
}

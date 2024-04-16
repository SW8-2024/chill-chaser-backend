using ChillChaser.Models.DB;

namespace ChillChaser.Services {
	public interface IWatchAuthService {
		public Task GrantAccess(string sessionToken, string userId);
		public Task<string> WaitForAuth(string sessionToken);
	}
}

namespace ChillChaser.Models.Exceptions {
	public class TokenAlreadyUsedException : Exception {
		public TokenAlreadyUsedException(string? message) : base(message) {
		}
	}
}

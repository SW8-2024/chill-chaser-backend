namespace ChillChaser.Models.Exceptions {
	public class UnknownTokenException : Exception {
		public UnknownTokenException(string? message) : base(message) {
		}
	}
}

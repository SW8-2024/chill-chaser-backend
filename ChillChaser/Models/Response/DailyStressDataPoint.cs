namespace ChillChaser.Models.Response {
	public class DailyStressDataPoint {
		public required int Value { get; set; }
		public required DateOnly Date { get; set; }
	}
}

namespace ChillChaser.Models.Response {
	public class GetStressMetricsResponse {
		public required double Min { get; set; }
		public required double Average { get; set; }
		public required double Max { get; set; }
		public required double Latest { get; set; }
	}
}

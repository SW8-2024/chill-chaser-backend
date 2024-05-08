namespace ChillChaser.Models.Response {
	public class GetBreakdownDataResponse {
		public required double AverageStress { get; set; }
		public required IEnumerable<StressByAppDataPoint> StressByApp { get; set; }
		public required IEnumerable<DailyStressDataPoint> DailyStressDataPoints { get; set; }
	}
}

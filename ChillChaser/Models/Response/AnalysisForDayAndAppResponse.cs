namespace ChillChaser.Models.Response {
	public class AnalysisForDayAndAppResponse {
		public required IEnumerable<StressValueAndUsage> HighResolutionStress { get; set; }
		public required IEnumerable<AppUsageAnalysis> AppUsageAnalysis { get; set; }
	}
}

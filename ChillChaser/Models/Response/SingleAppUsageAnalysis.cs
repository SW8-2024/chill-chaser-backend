namespace ChillChaser.Models.Response {
	public class SingleAppUsageAnalysis {
		public required double AverageStress { get; set; }
		public required double ReferenceStress { get; set; }
		public required TimeSpan Usage { get; set; }
	}
}

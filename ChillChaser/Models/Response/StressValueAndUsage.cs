namespace ChillChaser.Models.Response {
	public class StressValueAndUsage {
		public required double AverageStress { get; set; }
		public required DateTime DateTime { get; set; }
		public required bool AppOpen { get; set; }
	}
}

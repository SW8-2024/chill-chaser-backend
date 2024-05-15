namespace ChillChaser.Models.Response {
	public class AppUsageForAppAndDay {
		public required DateTime DateTime { get; set; }
		public required double DayReferenceStress { get; set; }
		public required double DayAverageStress { get; set; }
		public required TimeSpan TotalUsage { get; set; }
	}
}

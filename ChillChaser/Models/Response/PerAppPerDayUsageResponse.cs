namespace ChillChaser.Models.Response {
	public class PerAppPerDayUsageResponse {
		public required IEnumerable<AppUsageForAppAndDay> AppUsageForAppAndDays { get; set; }
	}
}

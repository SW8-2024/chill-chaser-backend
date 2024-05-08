using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;

namespace ChillChaser.Services {
	public interface IBreakDownService {
		DateOnlyRange GetLastMonthRange(DateTime now);
		DateOnlyRange GetTodayRange(DateTime now);
		Task<IEnumerable<StressByAppDataPoint>> GetStressByApp(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<IEnumerable<DailyStressDataPoint>> GetDailyStress(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<StressMetrics> GetStressMetrics(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<double> GetLatestHeartRate(CCDbContext ctx, string userId);
	}
}

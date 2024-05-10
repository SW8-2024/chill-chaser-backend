﻿using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;

namespace ChillChaser.Services {
	public interface IAnalysisService {
		DateOnlyRange GetLastMonthRange(DateTime now);
		DateOnlyRange GetTodayRange(DateTime now);
		Task<IEnumerable<StressByAppDataPoint>> GetStressByApp(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<IEnumerable<DailyStressDataPoint>> GetDailyStress(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<StressMetrics> GetStressMetrics(CCDbContext ctx, string userId, DateOnlyRange dateRange);
		Task<LatestHeartRate> GetLatestHeartRate(CCDbContext ctx, string userId);
		Task<IEnumerable<AppUsageAnalysis>> GetAppUsageAnalysis(CCDbContext ctx, string userId);
	}
}

using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChillChaser.Services.impl {
	public class BreakDownService : IBreakDownService {
		public DateOnlyRange GetLastMonthRange(DateTime date) {
			var endTime = new DateOnly(date.Year, date.Month, date.Day).AddDays(1);
			var startTime = endTime.AddMonths(-1);
			return new DateOnlyRange {
				From = startTime,
				To = endTime
			};
		} 

		public async Task<StressMetrics> GetStressMetrics(CCDbContext ctx, string userId, DateOnlyRange dateRange) {
			return await ctx.Database.SqlQuery<StressMetrics?>($"""
			SELECT 
				COALESCE(AVG(hr."Bpm"), 0) AS "Average",
				COALESCE(MIN(hr."Bpm"), 0) AS "Min",
				COALESCE(MAX(hr."Bpm"), 0) as "Max"
			FROM "HeartRates" hr
			WHERE hr."DateTime" > {dateRange.From} AND hr."DateTime" < {dateRange.To}
				AND hr."UserId" = {userId}
			""").SingleOrDefaultAsync() ?? new StressMetrics {
				Average = 0,
				Min = 0,
				Max = 0
			};
		}

		public async Task<double> GetLatestHeartRate(CCDbContext ctx, string userId) {
			return await ctx.Database.SqlQuery<double?>($"""
			SELECT hr."Bpm" AS "Value"
			FROM "HeartRates" hr
			WHERE hr."UserId" = {userId}
			ORDER BY hr."DateTime" DESC
			LIMIT 1
			""").SingleOrDefaultAsync() ?? 0;
		}

		//TODO: Test
		public async Task<IEnumerable<DailyStressDataPoint>> GetDailyStress(CCDbContext ctx, string userId, DateOnlyRange dateRange) {
			var dataWithHoles = await ctx.Database.SqlQuery<DailyStressDataPoint>($"""
			SELECT series.interval_begin AS "Date", hr_data.hr_avg as "Value"
			FROM (
				SELECT 
					generate_series({dateRange.From}, {dateRange.To} - interval '1 day', interval '1 day') AS interval_begin,
					generate_series({dateRange.From} + interval '1 day', {dateRange.To}, interval '1 day') AS interval_end
			) series
			CROSS JOIN LATERAL (
				SELECT
					COALESCE(AVG(hr."Bpm"), 0) as hr_avg
				FROM
					"HeartRates" hr
				WHERE 
					hr."DateTime" > series.interval_begin 
					AND hr."DateTime" < (series.interval_end)
					AND hr."UserId" = {userId}
			) hr_data;
			""").ToListAsync();
			return dataWithHoles;
		}

		public async Task<IEnumerable<StressByAppDataPoint>> GetStressByApp(CCDbContext ctx, string userId, DateOnlyRange dateRange) {
			return await ctx.Database.SqlQuery<StressByAppDataPoint>($"""
			SELECT 
				a."Name" as "Name", AVG(hr."Bpm") AS "Value"
			FROM "AppUsages" au
				JOIN "HeartRates" hr ON hr."DateTime" >= au."From" AND hr."DateTime" <= au."To"
				JOIN "Apps" a ON a."Id" = au."AppId"
			WHERE au."From" >= {dateRange.From} AND au."To" <= {dateRange.To} 
				AND au."UserId" = {userId} AND hr."UserId" = {userId}
			GROUP BY au."AppId", a."Name"
			""").ToListAsync();
		}

		public async Task<IEnumerable<AppUsageAnalysis>> GetAppUsageAnalysis(CCDbContext ctx, string userId)
		{
			return await ctx.Database.SqlQuery<AppUsageAnalysis>($"""
			SELECT 
				a."Name" as "Name",
				COALESCE(AVG(hr."Bpm"), 0) AS "AverageStress",
				COALESCE(AVG(stress_ref."ref_stress"), 0) AS "ReferenceStress",
				app_usage."usage" AS "Usage"
			FROM "AppUsages" au
			LEFT JOIN "HeartRates" hr ON 
				hr."DateTime" >= au."From" AND hr."DateTime" <= au."To"
			LEFT JOIN (
				SELECT 
					au."Id" AS "id",
					AVG(hr."Bpm") as ref_stress
				FROM "AppUsages" au
				LEFT JOIN "HeartRates" hr ON 
					hr."DateTime" > (au."From" - INTERVAL '1 hour') 
					AND hr."DateTime" < au."From"
				WHERE au."UserId" = {userId} AND hr."UserId" = {userId}
				GROUP BY
					au."Id"
			) stress_ref ON 
				stress_ref."id" = au."Id"
			LEFT JOIN (
				SELECT
					au."AppId" AS "id",
					SUM(AGE(au."To", au."From")) AS "usage"
				FROM "AppUsages" au
				WHERE au."UserId" = {userId}
				GROUP BY au."AppId"
			) app_usage ON 
				app_usage."id" = au."AppId"
			JOIN "Apps" a ON a."Id" = au."AppId"
			WHERE hr."UserId" = {userId} AND au."UserId" = {userId}
			GROUP BY au."AppId", a."Name", app_usage."usage"
			""").ToListAsync();
		}

		public DateOnlyRange GetTodayRange(DateTime now) {
			var startTime = new DateOnly(now.Year, now.Month, now.Day);
			var endTime = startTime.AddDays(1);
			return new DateOnlyRange() {
				From = startTime,
				To = endTime
			};
		}
	}
}

using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ChillChaser.Services.impl {
	public class AnalysisService(IAppService appService) : IAnalysisService {
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

		public async Task<LatestHeartRate> GetLatestHeartRate(CCDbContext ctx, string userId) {
			return await ctx.Database.SqlQuery<LatestHeartRate?>($"""
				SELECT hr."Bpm" AS "Value", hr."DateTime"
				FROM "HeartRates" hr
				WHERE hr."UserId" = {userId}
				ORDER BY hr."DateTime" DESC
				LIMIT 1
			""").SingleOrDefaultAsync() ?? new LatestHeartRate {
				Value = 0,
				DateTime = DateTime.MinValue
			};
		}

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
			) hr_data
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
			ORDER BY AVG(hr."Bpm") DESC
			""").ToListAsync();
		}

		public async Task<IEnumerable<AppUsageAnalysis>> GetAppUsageAnalysis(CCDbContext ctx, string userId)
		{
			return await ctx.Database.SqlQuery<AppUsageAnalysis>($"""
			SELECT 
				a."Name" as "Name",
				COALESCE(AVG(hr."Bpm"), 0) AS "AverageStress",
				COALESCE(AVG(rs."ReferenceStress"), 0) AS "ReferenceStress",
				app_usage."usage" AS "Usage"
			FROM "AppUsages" au
			LEFT JOIN "HeartRates" hr ON 
				hr."DateTime" >= au."From"
				AND hr."DateTime" <= au."To"
				AND hr."UserId" = au."UserId"
			LEFT JOIN "ReferencialStress" rs ON 
				rs."AppUsageId" = au."Id"
			LEFT JOIN (
				SELECT
					au."AppId" AS "id",
					SUM(AGE(au."To", au."From")) AS "usage"
				FROM "AppUsages" au
				WHERE au."UserId" = {userId}
				GROUP BY au."AppId"
			) app_usage ON 
				app_usage."id" = au."AppId"
			JOIN "Apps" a ON
				a."Id" = au."AppId"
			WHERE
				au."UserId" = {userId}
			GROUP BY
				au."AppId",
				a."Name",
				app_usage."usage"
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

		public async Task<IEnumerable<AppUsageForAppAndDay>> GetAppPerDayUsageAnalysis(CCDbContext ctx, string userId, DateTime endOfDay, string appName) {
			return await ctx.Database.SqlQuery<AppUsageForAppAndDay>($"""
			SELECT 
				series.interval_begin AS "DateTime",
				"data"."ReferenceStress" AS "DayReferenceStress",
				"data"."AverageStress" AS "DayAverageStress",
				"data"."TotalUsage" AS "TotalUsage"
			FROM (
				SELECT 
					generate_series({endOfDay} - interval '30 days', {endOfDay} - interval '24 hours', interval '24 hours') AS interval_begin,
					generate_series(({endOfDay} - interval '30 days') + interval '24 hours', {endOfDay}, interval '24 hours') AS interval_end
			) series
			CROSS JOIN LATERAL (
				SELECT
					AVG(rs."ReferenceStress") as "ReferenceStress",
					AVG(aus."AverageStress") AS "AverageStress",
					SUM(AGE(au."To", au."From")) AS "TotalUsage"
				FROM
					"AppUsages" au
				LEFT JOIN "AppUsageStress" aus ON
					aus."AppUsageId" = au."Id"
				LEFT JOIN "ReferencialStress" rs ON
					rs."AppUsageId" = au."Id"
				LEFT JOIN "Apps" a ON
					a."Id" = au."AppId"
				WHERE
					au."From" >= series.interval_begin AND
					au."From" <= series.interval_end AND
					au."UserId" = {userId} AND
					a."Name" = {appName}
			) "data"
			WHERE "data"."ReferenceStress" IS NOT NULL AND "data"."AverageStress" IS NOT NULL
			ORDER BY series.interval_begin
			""").ToListAsync();
		}

		public async Task<IEnumerable<SingleAppUsageAnalysis>> GetAppUsageForDay(CCDbContext ctx, string userId, DateTime endOfDay, string appName) {
			return await ctx.Database.SqlQuery<SingleAppUsageAnalysis>($"""
			SELECT 
				au."Id",
				au."From" as "AppUsageStart",
				rs."ReferenceStress",
				aus."AverageStress",
				AGE(au."To", au."From") as "Usage"
			FROM "AppUsages" au
			LEFT JOIN "ReferencialStress" rs ON 
				rs."AppUsageId" = au."Id"
			LEFT JOIN "AppUsageStress" aus ON
				aus."AppUsageId" = au."Id"
			LEFT JOIN "Apps" a ON
				a."Id" = au."AppId"
			WHERE 
				au."UserId" = {userId} AND
				au."From" >= ({endOfDay} - interval '24 hours') AND
				au."From" <= {endOfDay} AND
				a."Name" = {appName}
			GROUP BY au."Id", au."To", au."From", rs."ReferenceStress", aus."AverageStress", a."Name"
			HAVING aus."AverageStress" IS NOT NULL
			ORDER BY "AppUsageStart" DESC
			""").ToListAsync();

		}

		public async Task<IEnumerable<StressValueAndUsage>> GetHighResolutionStressForDayAndApp(CCDbContext ctx, string userId, string appName, DateTime endOfDay) {
			return await ctx.Database.SqlQuery<StressValueAndUsage>($"""
			SELECT 
				series.interval_begin AS "DateTime",
				hr_data."AppOpen",
				hr_data."AverageStress"
			FROM (
				SELECT 
					generate_series({endOfDay} - interval '24 hours', {endOfDay} - interval '15 minutes', interval '15 minutes') AS interval_begin,
					generate_series(({endOfDay} - interval '24 hours') + interval '15 minutes', {endOfDay}, interval '15 minutes') AS interval_end
			) series
			CROSS JOIN LATERAL (
				SELECT
					COALESCE(AVG(hr."Bpm"), 0) as "AverageStress",
					COALESCE(bool_or(hr."AppOpen"), FALSE) as "AppOpen"
				FROM
					(
						SELECT hr."Id", hr."Bpm", hr."UserId", a."Name" AS "AppName", hr."DateTime", bool_or(au."Id" IS NOT NULL) AS "AppOpen" FROM 
							"HeartRates" hr
						LEFT JOIN "AppUsages" au ON
							au."From" <= hr."DateTime" AND au."To" >= hr."DateTime" AND hr."UserId" = au."UserId"
						LEFT JOIN "Apps" a ON
							a."Id" = au."AppId"
						GROUP BY
							hr."Id", hr."Bpm", hr."UserId", hr."DateTime", a."Name"
					) hr
				WHERE
					hr."DateTime" >= series.interval_begin
					AND hr."DateTime" <= series.interval_end
					AND hr."UserId" = {userId}
			) hr_data
			ORDER BY series.interval_begin
			""").ToListAsync();
		}

		public async Task RefreshAnalysis(CCDbContext ctx) {
			await ctx.Database.ExecuteSqlAsync($"""
				REFRESH MATERIALIZED VIEW "ReferencialStress";
				REFRESH MATERIALIZED VIEW "AppUsageStress";
			""");
		}
	}
}

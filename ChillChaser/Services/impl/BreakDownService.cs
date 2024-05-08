using ChillChaser.Models.Internal;
using ChillChaser.Models.Response;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChillChaser.Services.impl {
	public class BreakDownService : IBreakDownService {
		private void PadDates(List<DailyStressDataPoint> stressPoints, DateOnly from, DateOnly to) {
			var index = 0;
			for (DateOnly current = from; current < to; current = current.AddDays(1)) {
				var currentObj = new DailyStressDataPoint {
					Date = current,
					Value = 0
				};

				if (index >= stressPoints.Count) {
					stressPoints.Add(currentObj);
					index++;
				} else if (current < stressPoints[index].Date) {
					stressPoints.Insert(index, currentObj);
					index++;
				} else if (current >= stressPoints[index].Date) {
					index++;
				}
			}
		}

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

		public async Task<IEnumerable<DailyStressDataPoint>> GetDailyStress(CCDbContext ctx, string userId, DateOnlyRange dateRange) {
			var dataWithHoles = await ctx.Database.SqlQuery<DailyStressDataPoint>($"""
				SELECT 
					hr."DateTime"::DATE AS "Date", AVG(hr."Bpm") AS "Value"
				FROM "HeartRates" hr
				WHERE hr."DateTime" > {dateRange.From} AND hr."DateTime" < {dateRange.To}
					AND hr."UserId" = {userId}
				GROUP BY hr."DateTime"::DATE
				ORDER BY "Date" ASC
			""").ToListAsync();
			PadDates(dataWithHoles, dateRange.From, dateRange.To);
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

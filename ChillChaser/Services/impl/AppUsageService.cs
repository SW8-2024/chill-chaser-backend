
using System.ComponentModel.DataAnnotations;
using System.Data;
using ChillChaser.Models.DB;
using ChillChaser.Models.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace ChillChaser.Services.impl {
	public class AppUsageService(IAppService appService) : IAppUsageService {
		public async Task AddAppUsage(CCDbContext ctx, IEnumerable<CreateAppUsage> appUsages, string userId) {

			await ctx.Database.OpenConnectionAsync();
			var conn = ctx.Database.GetDbConnection();

			using var tx = ctx.Database.BeginTransaction();
			
			var relevantApps = await appService.CreateOrGetApps(ctx, appUsages.Select(au => au.AppName).ToHashSet());
			await ctx.SaveChangesAsync();

			using var command = ctx.Database.GetDbConnection().CreateCommand();
			
			command.Transaction = tx.GetDbTransaction();
			command.CommandText = """
				INSERT INTO "AppUsages" ("From", "To", "AppId", "UserId")
					VALUES (@From, @To, @AppId, @UserId)
				ON CONFLICT ("From", "AppId", "UserId") DO 
					UPDATE SET "To" = GREATEST("AppUsages"."To", EXCLUDED."To")
			""";
			command.Parameters.Add(new NpgsqlParameter("@From", DbType.DateTime));
			command.Parameters.Add(new NpgsqlParameter("@To", DbType.DateTime));
			command.Parameters.Add(new NpgsqlParameter("@AppId", DbType.Int32));
			command.Parameters.Add(new NpgsqlParameter("@UserId", DbType.String));
			
			foreach (var appUsage in appUsages) {
				var app = relevantApps[appUsage.AppName] ?? throw new Exception("Missing apps after create or get");
				foreach (var session in appUsage.Sessions) {
					command.Parameters[0].Value = session.From;
					command.Parameters[1].Value = session.To;
					command.Parameters[2].Value = app.Id;
					command.Parameters[3].Value = userId;
					await command.ExecuteNonQueryAsync();
				}
				
			}
			
			await tx.CommitAsync();
		}
	}
}

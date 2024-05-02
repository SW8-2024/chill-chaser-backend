using ChillChaser.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace ChillChaser.Services.impl {
	public class AppService : IAppService {
		public async Task<App> CreateOrGetApp(CCDbContext ctx, string appName) {
			var app = await ctx.Apps.SingleOrDefaultAsync(a => a.Name == appName);

			if (app == null) {
				app = CreateApp(ctx, appName);
			}

			return app;
		}

		public async Task<IDictionary<string, App>> CreateOrGetApps(CCDbContext ctx, ISet<string> appNames) {
			var apps = from app in ctx.Apps
					   where appNames.Contains(app.Name)
					   select app;
			var appDictionary = await apps.ToDictionaryAsync(app => app.Name);
			foreach (var appName in appNames) {
				if (!appDictionary.ContainsKey(appName)) {
					appDictionary.Add(appName, CreateApp(ctx, appName));
				}
			}
			return appDictionary;
		}

		private App CreateApp(CCDbContext ctx, string appName) {
			var app = new App { Name = appName };
			ctx.Apps.Add(app);
			return app;
		}
	}
}

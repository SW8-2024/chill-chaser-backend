using ChillChaser.Services;

namespace ChillChaser.HostedService
{
    public class RefreshAnalysisHostedService(ILogger<RefreshAnalysisHostedService> logger, IAnalysisService analysisService, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Refresh Analysis Hosted Service running.");

            using PeriodicTimer timer = new(TimeSpan.FromMinutes(5));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await DoWork();
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Refresh Analysis Hosted Service is stopping.");
                }
                catch (Exception e)
                {
                    logger.LogError("Refresh analysis failed: {Error}", e);
                }
            }
        }

        private async Task DoWork()
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            CCDbContext ctx = scope.ServiceProvider.GetRequiredService<CCDbContext>();
            await analysisService.RefreshAnalysis(ctx);
        }
    }
}

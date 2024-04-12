namespace ChillChaser.Services
{
    public interface IHeartRateService
    {
        Task AddHeartRate(CCDbContext ctx, int bpm, DateTime dateTime, string userID);
    }
}
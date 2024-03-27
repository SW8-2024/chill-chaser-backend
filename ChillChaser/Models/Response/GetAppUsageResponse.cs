namespace ChillChaser.Models.Response
{
    public class GetAppUsageResponse
    {
        public required int Id { get; set; }
        public required string AppName { get; set; }
        public required DateTime From { get; set; }
        public required DateTime To { get; set; }
    }
}

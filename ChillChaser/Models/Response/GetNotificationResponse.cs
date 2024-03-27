namespace ChillChaser.Models.Response
{
    public class GetNotificationResponse
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime ReceivedAt { get; set; }
        public required string AppName { get; set; }
    }
}

namespace ChillChaser.Models.Request
{
    public class CreateNotification
    {
        public required string AppName { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required DateTime ReceivedAt { get; set; }
    }
}

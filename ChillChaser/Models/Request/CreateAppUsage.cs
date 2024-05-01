namespace ChillChaser.Models.Request
{
    public class CreateAppUsage
    {
        public required DateTime From { get; set; }
        public required DateTime To { get; set; }
        public required  IEnumerable<AppSession> Sessions {get; set;}
        public required string AppName { get; set; }
    }
}

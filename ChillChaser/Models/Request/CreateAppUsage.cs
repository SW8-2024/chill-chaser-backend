namespace ChillChaser.Models.Request
{
    public class CreateAppUsage
    {
        public required  IEnumerable<AppSession> Sessions {get; set;}
        public required string AppName { get; set; }
    }
}

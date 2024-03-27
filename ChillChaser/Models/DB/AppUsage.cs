namespace ChillChaser.Models.DB
{
    public class AppUsage
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int AppId { get; set; }
        public App App { get; set; }
        public string UserId { get; set; }
        public CCUser User { get; set; }
    }
}

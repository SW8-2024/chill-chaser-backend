namespace ChillChaser.Models.Request
{
    public class CreateHeartRate
    {
        public required int Bpm {get; set;}
        public required DateTime DateTime {get; set;}
    }
}
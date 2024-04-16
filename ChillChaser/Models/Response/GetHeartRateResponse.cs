using ChillChaser.Models.DB;

namespace ChillChaser.Models.Response
{
    public class HeartRateResponse
    {
        public int Id { get; set; }
        public int Bpm { get; set; }
        public DateTime DateTime { get; set; }
        public string UserId { get; set; }
    }
    public class GetHeartRateResponse
    {
        public List<HeartRateResponse> HeartRates { get; set; }
    }
}
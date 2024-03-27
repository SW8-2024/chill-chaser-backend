using Microsoft.Extensions.Hosting;

namespace ChillChaser.Models.DB
{
    public class App
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Notification> Notifications { get; set; } = [];

        public ICollection<AppUsage> AppUsage { get; set; } = [];
    }
}

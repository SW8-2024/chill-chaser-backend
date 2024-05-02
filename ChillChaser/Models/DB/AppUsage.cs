using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ChillChaser.Models.DB
{
	[Index(nameof(From), nameof(AppId), nameof(UserId), IsUnique = true, Name = "from_app_user_ui")]
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

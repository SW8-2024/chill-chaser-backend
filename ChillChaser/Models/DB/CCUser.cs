using Microsoft.AspNetCore.Identity;

namespace ChillChaser.Models.DB
{
    public class CCUser : IdentityUser
    {
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<AppUsage> AppUsage { get; set; }
    }
}

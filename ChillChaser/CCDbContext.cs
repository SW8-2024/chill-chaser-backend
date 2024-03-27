using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChillChaser.Models.DB;

namespace ChillChaser
{
    public class CCDbContext(DbContextOptions<CCDbContext> options) 
        : IdentityDbContext<CCUser>(options)
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AppUsage> AppUsages { get; set; }
        public DbSet<App> Apps { get; set; }
    }
}

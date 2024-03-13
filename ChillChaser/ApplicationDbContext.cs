using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChillChaser.Models.DB;

namespace ChillChaser
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<App> Apps { get; set; }
    }
}

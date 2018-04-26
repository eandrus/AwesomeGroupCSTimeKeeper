using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CSTimeKeeper.Models;

namespace CSTimeKeeper.Data
{
    public class TimeKeeperDbContext : IdentityDbContext<User>
    {
        public DbSet<User> User { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<StudentCourse> StudentCourse { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<GroupMember> GroupMember { get; set; }
        public DbSet<TimeEntry> TimeEntry { get; set; }
        public DbSet<TimeEntryChange> TimeEntryChange { get; set; }

        public TimeKeeperDbContext(DbContextOptions<TimeKeeperDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Course>().Property("Active").HasDefaultValue(true);
            builder.Entity<Project>().Property("Active").HasDefaultValue(true);
            builder.Entity<StudentCourse>().Property("Approved").HasDefaultValue(false);
            builder.Entity<User>().Property("Administrator").HasDefaultValue(false);
            base.OnModelCreating(builder);
        }
    }
}

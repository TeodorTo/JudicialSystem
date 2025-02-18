using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Judicial_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Submission>()
                .HasOne(s => s.Task)
                .WithMany()
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Submission>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Submission>()
                .Property(s => s.Score)
                .HasPrecision(18, 2);

        }
    }
}

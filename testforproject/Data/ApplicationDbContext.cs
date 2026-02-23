using Microsoft.EntityFrameworkCore;
using testforproject.Models;

namespace testforproject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Requirements> Requirements { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Following)
                .WithMany(u => u.Follower)
                .UsingEntity(j => j.ToTable("UserFollows"));

            
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Owner)
                .WithMany(u => u.OwningEvent)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Particitpant)
                .WithMany(u => u.ParticipatedEvent)
                .UsingEntity<Dictionary<string, object>>(
                    "EventUser",
                    j => j
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("ParticitpantUid")
                        .OnDelete(DeleteBehavior.NoAction),
                    j => j
                        .HasOne<Event>()
                        .WithMany()
                        .HasForeignKey("Eid")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        }




    }
}

using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
        public DbSet<Category> Categories { get; set; }

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
                .HasMany(e => e.Participants)
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
           

            // 2. จัดการความสัมพันธ์ตาราง Category แบบ Many-to-Many (ส่วนนี้คือโค้ดใหม่ที่ถูกต้อง)
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Categories)
                .WithMany(c => c.Events)
                .UsingEntity(j => j.ToTable("EventCategories"));
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Board Games" },
                new Category { Id = 2, Name = "PC Gaming" },
                new Category { Id = 3, Name = "Console Gaming" },
                new Category { Id = 4, Name = "Tabletop RPG" },
                new Category { Id = 5, Name = "Cafe Hopping" },
                new Category { Id = 6, Name = "Movie Night" },
                new Category { Id = 7, Name = "Karaoke" },
                new Category { Id = 8, Name = "Escape Room" },
                new Category { Id = 9, Name = "Theme Park" },
                new Category { Id = 10, Name = "Camping" },
                new Category { Id = 11, Name = "Hiking" },
                new Category { Id = 12, Name = "Cycling" },
                new Category { Id = 13, Name = "Football" },
                new Category { Id = 14, Name = "Basketball" },
                new Category { Id = 15, Name = "Badminton" },
                new Category { Id = 16, Name = "Bowling" },
                new Category { Id = 17, Name = "Gym & Fitness" },
                new Category { Id = 18, Name = "Street Food" },
                new Category { Id = 19, Name = "Fine Dining" },
                new Category { Id = 20, Name = "Pub Crawl" },
                new Category { Id = 21, Name = "BBQ / Grill" },
                new Category { Id = 22, Name = "Concert & Live Music" },
                new Category { Id = 23, Name = "Museum / Art Gallery" },
                new Category { Id = 24, Name = "Photography" },
                new Category { Id = 25, Name = "Shopping" },
                new Category { Id = 26, Name = "Road Trip" },
                new Category { Id = 27, Name = "Spa Day" },
                new Category { Id = 28, Name = "Volunteer" },
                new Category { Id = 29, Name = "Cooking Class" },
                new Category { Id = 30, Name = "Hackathon / Coding" }
            );
        }




    }
}

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
        public DbSet<EventScore> EventScores { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<ParticipantConfirmation> ParticipantConfirmations { get; set; }

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
                new Category { Id = 1, Name = "Sports & Fitness" ,ImageUrl= "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRfY6dGXDvuNyxepwwRCDD4aI8MmrroG4Xj8g&s" },
                new Category { Id = 2, Name = "Gaming & eSports", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRxoudo-kmIpvw6ATdSFlKh03M2tIMw1P6Jbw&s" },
                new Category { Id = 4, Name = "Technology & Coding", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTUXSwrWPMPioZYdGqVU1dBR75K0bNYuixisQ&s" },
                new Category { Id = 5, Name = "Education & Learning", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQB_3U8P0eFjlVTUUZtAx2Be8ob_2HiFtH68Q&s" },
                new Category { Id = 6, Name = "Arts & Crafts", ImageUrl = "https://voca-land.sgp1.cdn.digitaloceanspaces.com/43844/1723178338611/a55b073aef1b83a4ccf3a83be979de70.jpg" },
                new Category { Id = 7, Name = "Food & Drink", ImageUrl = "https://i.shgcdn.com/3889bf5d-9000-4cbc-a021-cd6051095102/-/format/auto/-/preview/3000x3000/-/quality/lighter/" },
                new Category { Id = 8, Name = "Travel & Outdoors", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSWlwvLgv80oIsBgM73ux9uS1qWkrduUPplnQ&s" },
                new Category { Id = 9, Name = "Health & Wellness", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQbWVzCwBNaRL8UYHtc_4_MOL6ZBeRmdH2S3g&s" },
                new Category { Id = 10, Name = "Networking & Business", ImageUrl = "https://www.abundance.global/wp-content/uploads/2024/06/business-networking-1080x675-1.jpeg" },
                new Category { Id = 11, Name = "Music & Concerts", ImageUrl = "https://img.jakpost.net/c/2019/06/12/2019_06_12_74202_1560308728._large.jpg" },
                new Category { Id = 12, Name = "Movies & Theater", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQAFJZdr-RTJ5_Sa_unf88P4OIW0dXyYcDcFQ&s" },
                new Category { Id = 13, Name = "Photography & Video", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTdrWNEbhPUO60Zaz7LUDmVBbUmBLEWFX38JA&s" },
                new Category { Id = 14, Name = "Books & Writing", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSRJ88He6SLg3xF2KS-CfXE7pZJBo1r4PaHTA&s" },
                new Category { Id = 15, Name = "Language & Culture", ImageUrl = "https://carollaguirre.wordpress.com/wp-content/uploads/2013/11/tumblr_inline_mm0kys229c1qz4rgp.jpg" },
                new Category { Id = 16, Name = "Volunteering & Charity", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRV7uM9mwm9yq3LKp00n1qd0i96m9gpJ-2EUQ&s" },
                new Category { Id = 17, Name = "Pets & Animals", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSEKp_2e5feC9OmHADhkiPoniNzMpVSE6b7KA&s" },
                new Category { Id = 18, Name = "Fashion & Beauty", ImageUrl = "https://mpics.mgronline.com/pics/Images/566000002659701.JPEG" },
                new Category { Id = 19, Name = "Science & Research", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRLSugPQ0OSBDuPK25QJu8BplFKJwDn1zC3vQ&s" },
                new Category { Id = 20, Name = "History & Philosophy", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS5j3y16Pum3NPZThK9FERKVdBzv0jey5-hJA&s" },
                new Category { Id = 21, Name = "Parenting & Family", ImageUrl = "https://www.focusonthefamily.com/wp-content/uploads/2019/07/D119D43F1A57459B858B9A11EC84408A-1024x575.jpeg" },
                new Category { Id = 22, Name = "Spirituality & Beliefs", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTsdqbWrGfqPZuuSRdAcknqiofR2ibJoYBWPQ&s" },
                new Category { Id = 23, Name = "Cars & Motorcycles", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ3_kqEOFllhUa_JYjPsbvtdpKzPY6xPrkOgA&s" },
                new Category { Id = 24, Name = "Real Estate", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQE_Cbgok89HHW9w9ub5PlE9A0bEoJSVzWMZQ&s" },
                new Category { Id = 25, Name = "Career Development", ImageUrl = "https://capital-placement.com/wp-content/uploads/2020/12/career-development.png" },
                new Category { Id = 26, Name = "Politics & Society", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ3WKQeAR7thwxBTzYxQQsSRUBJCUSrXleCyQ&s" },
                new Category { Id = 27, Name = "Dancing & Performing", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRbY_kY6LSxZq4oEs_EXUpzua16XDf6QJWqJw&s" },
                new Category { Id = 28, Name = "Board Games & Tabletop", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQvK_JEMhb7owBgmSSTO4P_Z1lvxf36WFp7WQ&s" },
                new Category { Id = 29, Name = "DIY & Home Improvement", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZ9KrCcuANQ3OoN5Ju7OmUCBlI7Tyw8K_Neg&s" },
                new Category { Id = 30, Name = "Comedy & Improv", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTakHVTgT0jryCuvRX410HBBwTTghRYhXOVYg&s" },
                new Category { Id = 31, Name = "Environment & Nature", ImageUrl = "https://earth.org/wp-content/uploads/2023/03/Untitled-683-%C3%97-1024px-1024-%C3%97-683px-73.jpg" },
                new Category { Id = 32, Name = "Dating & Singles", ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQcdLgyLqF6dX_21oeEwpkjpCOT83E-gnL3Iw&s" }
            );
        }




    }
}

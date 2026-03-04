using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace testforproject.Models
{
    public class User
    {
        [Key]
        public int Uid { get; set; }

        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Password { get; set; }

        public string? Email { get; set; }
        public string? Gender { get; set; }
        [Range(1, 80)]
        public uint? Age { get; set; }
        public string? ProfilePictureSrc { get; set; }
        public int? HostScore { get; set; }
        public int? ParticipateScore { get; set; }

        // Added for Recommendation Engine
        public double? PopularityScore { get; set; } = 0.0;
        public virtual ICollection<Category>? PreferredCategories { get; set; } = new List<Category>();

        public ICollection<Event>? OwningEvent { get; set; }

        public ICollection<Event>? ParticipatedEvent { get; set; }

        public ICollection<User>? Follower { get; set; }

        public ICollection<User>? Following { get; set; }

        public ICollection<Notification>? Notifications { get; set; }
        [NotMapped]
        public string? ConfirmPassword { get; set; }

    }

}



using System.ComponentModel.DataAnnotations;

namespace testforproject.Models
{
    public class EventParticipant
    {
        [Required]
        public int Eid { get; set; }
        public Event Event { get; set; }

        [Required]
        public int ParticitpantUid { get; set; }
        public User Participant { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}

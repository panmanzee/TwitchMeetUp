using System.ComponentModel.DataAnnotations;

namespace testforproject.Models
{
    public class ParticipantConfirmation
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; }
        
        [Required]
        public int UserId { get; set; }
    }
}

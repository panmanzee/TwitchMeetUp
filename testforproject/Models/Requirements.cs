using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace testforproject.Models
{
    public class Requirements
    {
        [Required]
        public int RequirementsId { get; set; }
        [Required]
        public String Gender { get; set; }
        [Required]
        public String Age { get; set; }
        [Required]
        public int ParticipantScore { get; set; }



    }

}

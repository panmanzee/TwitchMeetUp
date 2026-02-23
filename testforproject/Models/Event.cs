using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace testforproject.Models
{
    public class Event
    {
        [Key]
        public int Eid { get; set; }//
        
        public string Name { get; set; }//
        
        public ICollection<String> Catagories { get; set; }//
        
        
        public ICollection<User> Particitpant { get; set; }
       
        public string Location { get; set; }//
        
        public int MaxParticitpant { get; set; }//
        
        public String DurationStart { get; set; }//

        public String DurationEnd { get; set; }

        public bool IsExpired => DateTimeOffset.Now > ExpiredDate; // Utc
        public DateTimeOffset ExpiredDate { get; set; }
        public bool Status { get; set; }
        public string Discription { get; set; }
        public Requirements requirements { get; set; }

        // gu เพิ่มเอง 
        [Required]
        public DateTime EventStart { get; set; }

        [Required]
        public DateTime EventStop { get; set; }
        
        

        [Required]
        public string status { get; set; } = "open";

        [Required]
        public string Decription { get; set; }
        public int OwnerId { get; set; }

        public User Owner { get; set; }
        public Requirements? requirements { get; set; }

    }

}

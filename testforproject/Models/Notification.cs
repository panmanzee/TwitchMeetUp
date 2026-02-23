using System.ComponentModel.DataAnnotations;

namespace testforproject.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public int UserUid { get; set; }
        public bool IsReaded { get; set; }
        public User User { get; set; }
    }
}

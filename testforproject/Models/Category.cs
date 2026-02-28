using System.ComponentModel.DataAnnotations;

namespace testforproject.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public string? ImageUrl { get; set; }
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}

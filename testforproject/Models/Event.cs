using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace testforproject.Models
{
    public class Event : IValidatableObject
    {
        [Key]
        public int Eid { get; set; }//

        public string Name { get; set; }//

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();



        public ICollection<User> Participants { get; set; } = new List<User>();

        public string Location { get; set; }//

        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be at least 1.")]
        public int MaxParticitpant { get; set; }//



        public bool IsExpired => DateTimeOffset.Now > ExpiredDate; // Utc
        public DateTimeOffset ExpiredDate { get; set; }


        //public Requirements requirements { get; set; }

        // gu เพิ่มเอง 
        [Required]
        public DateTime EventStart { get; set; }

        [Required]
        public DateTime EventStop { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var now = DateTimeOffset.Now;

            if (EventStart <= now)
            {
                yield return new ValidationResult(
                    "Event start must be in the future.",
                    new[] { nameof(EventStart) });
            }

            if (EventStop <= EventStart)
            {
                yield return new ValidationResult(
                    "EventStop must be later than EventStart",
                    new[] { nameof(EventStop) });
            }

            if (ExpiredDate <= EventStop)
            {
                yield return new ValidationResult(
                    "ExpiredDate must be after EventStop",
                    new[] { nameof(ExpiredDate) });
            }
            if (ExpiredDate <= now)
            {
                yield return new ValidationResult(
                    "Registration deadline must be in the future.",
                    new[] { nameof(ExpiredDate) });
            }
        }

        [Required]
        public string status { get; set; } = "open";

        [NotMapped]
        public string ComputedStatus
        {
            get
            {
                var now = DateTimeOffset.Now;

                if (MaxParticitpant > 0 && Participants?.Count >= MaxParticitpant)
                    return "closed";

                if (now >= EventStart && now <= EventStop)
                    return "ongoing";

                if (now > EventStop)
                    return "ended";

                return "open";
            }
        }

        [Required]
        public string Description { get; set; }
        public int OwnerId { get; set; }

        public User Owner { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }

}
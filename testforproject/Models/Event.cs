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



        public bool IsExpired => DateTimeOffset.UtcNow > ExpiredDate; // เทียบกับเวลาสากล (UTC) ให้ตรงกับ Controller
        public DateTimeOffset ExpiredDate { get; set; }


        //public Requirements requirements { get; set; }

        // gu เพิ่มเอง 
        [Required]
        public DateTime EventStart { get; set; }

        [Required]
        public DateTime EventStop { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1. งานต้องเลิกหลังจากเริ่ม
            if (EventStop <= EventStart)
            {
                yield return new ValidationResult(
                    "EventStop must be later than EventStart",
                    new[] { nameof(EventStop) });
            }

            // 2. ปิดรับสมัครต้องเกิดก่อนหรืองานเลิก (แปลง DateTime เป็น Offset เพื่อให้เทียบกันได้แบบไม่รวน)
            if (ExpiredDate > new DateTimeOffset(EventStop))
            {
                yield return new ValidationResult(
                    "ExpiredDate must be before or equal to EventStop",
                    new[] { nameof(ExpiredDate) });
            }

            // หมายเหตุ: เราเอาการเช็ค "เวลาต้องเป็นอนาคต" (now <= EventStart) ออกจากตรงนี้
            // เพื่อเปิดทางให้ผู้จัดสามารถกด Edit งานที่ถูกจัดไปแล้วในอดีตได้ โดยไม่โดน Error ขวาง
        }

        [Required]
        public string status { get; set; } = "open";

        [NotMapped]
        public string ComputedStatus
        {
            get
            {
                var now = DateTimeOffset.Now;

                // 1. ถ้าเวลาเลยตอนจบไปแล้ว = ended
                if (now > EventStop)
                    return "ended";

                // 2. ถ้างานกำลังจัดอยู่ = ongoing
                if (now >= EventStart && now <= EventStop)
                    return "ongoing";

                // 3. ถ้าระบบฐานข้อมูลถูกสั่งปิด หรือ หมดเวลา Expired ไปแล้ว หรือ คนเต็มโควต้าแล้ว = closed
                if (status == "closed" || IsExpired || (MaxParticitpant > 0 && Participants?.Count >= MaxParticitpant))
                    return "closed";

                // 4. ถ้าไม่เข้าเงื่อนไขบนเลย = open
                return "open";
            }
        }


        [Required]
        public string Description { get; set; }
        public int OwnerId { get; set; }

        public User? Owner { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }

}
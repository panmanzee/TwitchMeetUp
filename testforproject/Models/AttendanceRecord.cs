namespace testforproject.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset AttendedAt { get; set; } = DateTimeOffset.Now;
    }
}
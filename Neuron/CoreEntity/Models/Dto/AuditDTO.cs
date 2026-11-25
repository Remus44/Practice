namespace Interview.Models
{
    public class AuditDTO
    {
        public AuditDTO(int id, string @event, string detail, DateTime timestamp)
        {
            Id = id;
            Event = @event;
            Detail = detail;
            Timestamp = timestamp;
        }

        public int Id { get; set; }
        public  string Event { get; set; }
        public  string Detail { get; set; }
        public DateTime Timestamp { get; set; }

        
    }
}
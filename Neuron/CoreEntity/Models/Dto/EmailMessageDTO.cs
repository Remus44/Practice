using Microsoft.EntityFrameworkCore;

namespace Interview.Models
{
    public class EmailMessageDTO :BaseDTO
    {
        public EmailMessageDTO(string to, string subject, string body, bool isSent)
        {
            To = to;
            Subject = subject;
            Body = body;
            IsSent = isSent;
        }

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsSent { get; set; }
    }
}
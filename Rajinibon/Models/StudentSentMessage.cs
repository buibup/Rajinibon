using System;

namespace Rajinibon.Models
{
    public class StudentSentMessage
    {
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string Status { get; set; }
        public string SentType { get; set; }
        public DateTime SentTime { get; set; }
    }
}

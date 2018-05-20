using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Models
{
    public class StudentSentMessage
    {
        public int Id { get; set; }
        public int StudentCheckTimeId { get; set; }
        public StudentCheckTime StudentCheckTime  { get; set; }
        public string Status { get; set; }
        public DateTime? SentTime { get; set; }
    }
}

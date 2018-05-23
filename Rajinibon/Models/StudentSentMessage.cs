using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Models
{
    public class StudentSentMessage
    {
        [Key]
        public int Id { get; set; }
        public int StudentCheckTimeId { get; set; }
        public string EmpId { get; set; }
        public string Status { get; set; }
        public string SentType { get; set; }
        public DateTime SentTime { get; set; }

        public virtual StudentCheckTime StudentCheckTime { get; set; }
    }
}

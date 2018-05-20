using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Models
{
    public class StudentCheckTime
    {
        public int Id { get; set; }
        public decimal? CuserId { get; set; }
        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public DateTime ChkTime { get; set; }
    }
}

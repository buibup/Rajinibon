using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.DataAccess
{
    interface IDbfDataConnection
    {
        IEnumerable<StudentCheckTime> GetStudentCheckTimes(string rootPath, string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(DateTime date, TimeSpan startTime, TimeSpan endTime);
    }
}

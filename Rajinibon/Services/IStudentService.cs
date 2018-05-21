using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Services
{
    interface IStudentService
    {
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesEntry(string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesExit(string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        Task RemoveStudentPass();
    }
}

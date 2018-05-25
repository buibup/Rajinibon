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
        Task<Tuple<List<StudentCheckTime>, List<StudentCheckTime>>> GetStudentCheckTimesEntry(string date);
        Task<Tuple<List<StudentCheckTime>, List<StudentCheckTime>>> GetStudentCheckTimesExit(string date);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageEntryAsync(string date);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageExitAsync(string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        Task RemoveStudentsLess(string date);
        Task SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        Task SaveExceptionLog(Exception ex);
    }
}

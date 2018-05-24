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
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageEntryAsync(string date);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageExit(string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        Task RemoveStudentsLess(string deate);
        Task SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        Task SaveExceptionLog(Exception ex);
    }
}

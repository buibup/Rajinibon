using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.DataAccess
{
    interface IMySqlDataConnection
    {
        Task SaveStudentCheckTimes(IEnumerable<StudentCheckTime> models);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(string date, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentSentMessageAsync(IEnumerable<StudentSentMessage> models);
        void SaveStudentSentMessage(IEnumerable<StudentSentMessage> model);
        void SaveStudentSentMessage(StudentSentMessage models);
        IEnumerable<StudentSentMessage> GetStudentSentMessages(string date, TimeSpan timeStart, TimeSpan timeEnd);
        Task<List<StudentSentMessage>> GetStudentsSentMessageError();
        Task SaveExceptionLog(Exception ex);
        Task RemoveStudentsCheckTimeLess(string date);
        Task RemoveStudentsSentMessageLess(string date);
        Task RemoveSentMessageError();
        bool SentSuccess(string empId, SentType sentType);
    }
}

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
        void SaveStudentCheckTimes(IEnumerable<StudentCheckTime> models);
        IEnumerable<StudentCheckTime> GetStudentCheckTimes(string date, TimeSpan timeStart, TimeSpan timeEnd);
        void SaveStudentSentMessageAsync(IEnumerable<StudentSentMessage> models);
        void SaveStudentSentMessage(IEnumerable<StudentSentMessage> model);
        void SaveStudentSentMessage(StudentSentMessage models);
        IEnumerable<StudentSentMessage> GetStudentSentMessages(string date, TimeSpan timeStart, TimeSpan timeEnd);
        List<StudentSentMessage> GetStudentsSentMessageError();
        void SaveExceptionLog(Exception ex);
        void RemoveStudentsCheckTimeLess(string date);
        void RemoveStudentsSentMessageLess(string date);
        void RemoveSentMessageError();
        bool SentSuccess(string empId, SentType sentType);
    }
}

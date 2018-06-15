using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Services
{
    public interface IStudentService
    {
        Tuple<List<StudentCheckTime>, List<StudentCheckTime>> GetStudentCheckTimesEntry(string date);
        Tuple<List<StudentCheckTime>, List<StudentCheckTime>> GetStudentCheckTimesExit(string date);
        List<StudentCheckTime> GetStudentCheckTimesEntryMySql(string date);
        List<StudentCheckTime> GetStudentCheckTimesExitMySql(string date);
        List<StudentCheckTime> GetStudentsEntryDbf(string date);
        List<StudentCheckTime> GetStudentsExitDbf(string date);
        List<StudentCheckTime> GetStudentsEntryMySql(string date);
        List<StudentCheckTime> GetStudentsExitMySql(string date);
        List<StudentCheckTime> GetStudentsEntryFromList(List<StudentCheckTime> models);
        List<StudentCheckTime> GetStudentsExitFromList(List<StudentCheckTime> models);
        List<StudentSentMessage> GetStudentSentMessageEntryFromList(List<StudentSentMessage> models);
        List<StudentSentMessage> GetStudentsSentMessagesEntryFromMySql(string date);
        List<StudentSentMessage> GetStudentsSentMessagesExitFromMySql(string date);
        List<StudentSentMessage> GetStudentSentMessageExitFromList(List<StudentSentMessage> models);
        IEnumerable<StudentSentMessage> GetStudentSentMessageEntry(string date);
        IEnumerable<StudentSentMessage> GetStudentSentMessageExit(string date);
        IEnumerable<StudentCheckTime> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        IEnumerable<StudentSentMessage> GetStudentSentMessage(IEnumerable<StudentSentMessage> models, TimeSpan timeStart, TimeSpan timeEnd);
        void SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        void SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        void RemoveStudentsLess(string date);
        void SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        void SentStudentsNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        void SaveExceptionLog(Exception ex);
        bool SentMessageSuccess(IEnumerable<StudentCheckTime> models, SentType sentType);
        void RemoveSentMessageError();
        List<StudentSentMessage> GetStudentsSentMessageError();
    }
}

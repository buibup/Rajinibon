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
        Task<Tuple<List<StudentCheckTime>, List<StudentCheckTime>>> GetStudentCheckTimesEntry(string date);
        Task<Tuple<List<StudentCheckTime>, List<StudentCheckTime>>> GetStudentCheckTimesExit(string date);
        Task<List<StudentCheckTime>> GetStudentCheckTimesEntryMySql(string date);
        Task<List<StudentCheckTime>> GetStudentCheckTimesExitMySql(string date);
        Task<List<StudentCheckTime>> GetStudentsEntryDbf(string date);
        Task<List<StudentCheckTime>> GetStudentsExitDbf(string date);
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
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessage(IEnumerable<StudentSentMessage> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        Task RemoveStudentsLess(string date);
        void SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        void SentStudentsNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        Task SaveExceptionLog(Exception ex);
        bool SentMessageSuccess(IEnumerable<StudentCheckTime> models, SentType sentType);
        Task RemoveSentMessageError();
        Task<List<StudentSentMessage>> GetStudentsSentMessageError();
    }
}

﻿using Rajinibon.Models;
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
        Task<List<StudentCheckTime>> GetStudentsEntryDbf(string date);
        Task<List<StudentCheckTime>> GetStudentsExitDbf(string date);
        List<StudentCheckTime> GetStudentsEntryMySql(string date);
        List<StudentCheckTime> GetStudentsExitMySql(string date);
        List<StudentCheckTime> GetStudentsEntryFromList(List<StudentCheckTime> models);
        List<StudentCheckTime> GetStudentsExitFromList(List<StudentCheckTime> models);
        List<StudentSentMessage> GetStudentSentMessageEntryFromList(List<StudentSentMessage> models);
        List<StudentSentMessage> GetStudentSentMessageExitFromList(List<StudentSentMessage> models);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageEntryAsync(string date);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageExitAsync(string date);
        Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessage(IEnumerable<StudentSentMessage> models, TimeSpan timeStart, TimeSpan timeEnd);
        Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models);
        Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models);
        Task RemoveStudentsLess(string date);
        void SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType);
        Task SaveExceptionLog(Exception ex);
        bool SentMessageSuccess(IEnumerable<StudentCheckTime> models, SentType sentType);
    }
}

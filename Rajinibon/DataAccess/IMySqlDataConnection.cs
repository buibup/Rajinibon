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
        Task SaveStudentSentMessage(IEnumerable<StudentSentMessage> models);
        Task<IEnumerable<StudentSentMessage>> GetStudentSentMessages(string date, TimeSpan timeStart, TimeSpan timeEnd);
    }
}

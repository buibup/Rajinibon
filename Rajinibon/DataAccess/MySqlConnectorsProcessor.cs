using Rajinibon.Common;
using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.DataAccess
{
    public static class MySqlConnectorsProcessor
    {
        public static List<StudentCheckTime> GetStudentCheckTimes(this List<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentCheckTime>();

            foreach(var item in models)
            {
                var chkTime = item.ChkTime;
                var time = new TimeSpan(chkTime.Hour, chkTime.Minute, chkTime.Millisecond);

                if (time.IsBetween(timeStart, timeEnd))
                {
                    var model = new StudentCheckTime()
                    {
                        CuserId = item.CuserId,
                        EmpId = item.EmpId,
                        EmpName = item.EmpName,
                        ChkTime = item.ChkTime
                    };

                    results.Add(model);
                }
            }

            return results;
        }

        public static List<StudentSentMessage> GetStudentSentMessage(this List<StudentSentMessage> models, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentSentMessage>();

            foreach (var item in models)
            {
                var chkTime = item.ChkTime;
                var time = new TimeSpan(chkTime.Hour, chkTime.Minute, chkTime.Millisecond);

                if (time.IsBetween(timeStart, timeEnd))
                {
                    var model = new StudentSentMessage()
                    {
                        EmpId = item.EmpId,
                        Status = item.Status,
                        SentType = item.SentType,
                        SentTime = item.SentTime,
                        ChkTime = item.ChkTime
                    };

                    results.Add(model);
                }
            }

            return results;
        }
    }
}

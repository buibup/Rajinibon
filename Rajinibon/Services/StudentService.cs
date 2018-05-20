using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rajinibon.Common;
using Rajinibon.DataAccess;
using Rajinibon.Models;

namespace Rajinibon.Services
{
    public class StudentService : IStudentService
    {
        IDbfDataConnection DbfDataConnection { get; set; }

        public StudentService()
        {
            DbfDataConnection = new DbfConnector();
        }
        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesEntry(string date)
        {
            var result = new List<StudentCheckTime>();
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);
            var timeStart = new TimeSpan(5, 0, 0);
            var timeEnd = new TimeSpan(12, 0, 0);

            foreach(var item in data)
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
                        ChkTime = chkTime
                    };
                    result.Add(model);
                }
            }

            return result.StudentCheckTimesFirstEntry();
        }

        public Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesExit(string date)
        {
            throw new NotImplementedException();
        }

        public Task RemoveStudentPass()
        {
            throw new NotImplementedException();
        }

        public Task SaveStudentEntry(IEnumerable<StudentCheckTime> models)
        {
            throw new NotImplementedException();
        }

        public Task SaveStudentExit(IEnumerable<StudentCheckTime> models)
        {
            throw new NotImplementedException();
        }

        public Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models)
        {
            throw new NotImplementedException();
        }
    }
}

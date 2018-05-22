using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rajinibon.Common;
using Rajinibon.DataAccess;
using Rajinibon.Models;
using RestSharp;

namespace Rajinibon.Services
{
    public class StudentService : IStudentService
    {
        IDbfDataConnection DbfDataConnection { get; set; }
        IMySqlDataConnection MySqlDataConnection { get; set; }

        public StudentService()
        {
            DbfDataConnection = new DbfConnector();
            MySqlDataConnection = new MySqlConnectors();
        }
        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesEntry(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);
            var timeStart = new TimeSpan(5, 0, 0);
            var timeEnd = new TimeSpan(12, 0, 0);

            var studentsEntryDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);
            var studentsEntryDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            var results = studentsEntryDbf.Where(s => !studentsEntryDb.Any(s2 => s2.EmpId == s.EmpId));

            return results.StudentCheckTimesFirstTime();
        }

        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesExit(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);
            var timeStart = new TimeSpan(15, 0, 0);
            var timeEnd = new TimeSpan(18, 30, 0);

            var studentsExitDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);
            var studentsExitDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            var result = studentsExitDbf.Where(s => !studentsExitDb.Any(s2 => s2.EmpId == s.EmpId));

            return result.StudentCheckTimesFirstTime();
        }

        public Task RemoveStudentPass()
        {
            throw new NotImplementedException();
        }

        public async Task SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models)
        {
            await MySqlDataConnection.SaveStudentCheckTimes(models);
        }

        public Task SaveStudentSentMessage(IEnumerable<StudentCheckTime> models)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var result = new List<StudentCheckTime>();

            await Task.Run(() => 
            {
                foreach (var item in models)
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
            });


            return result;
        }

        public async Task SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models)
        {
            var url = GlobalConfig.AppSettings("sentMessageService")
                .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
                .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));

            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddBody("content-type", "application/form-data");

            request.AddParameter("students", "999902");
            request.AddParameter("message", "test");
            request.AddParameter("rooms", "");
            request.AddParameter("username", "0411");

            await Task.Run(() =>
             {
                 client.ExecuteAsync(request, response =>
                 {
                     var data = response.Content;
                 });
             });
        }
    }
}

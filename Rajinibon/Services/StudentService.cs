using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

            if (data.ToList().Count == 0) { return new List<StudentCheckTime>(); }

            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            var studentsEntryDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);
            var studentsEntryDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            var studentsEntry = studentsEntryDbf.Where(s => !studentsEntryDb.Any(s2 => s2.EmpId == s.EmpId));

            if (studentsEntry.ToList().Count > 0)
            {
                await SaveStudentStudentCheckTime(studentsEntry);
            }

            var studentsEntrySentMsg = await MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);
            var results = studentsEntryDbf.Where(s => !studentsEntrySentMsg.Any(s2 => s2.EmpId == s.EmpId));

            return results.StudentCheckTimesFirstTime();
        }

        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesExit(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0) { return new List<StudentCheckTime>(); }

            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            var studentsExitDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);
            var studentsExitDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            var studentsExit = studentsExitDbf.Where(s => !studentsExitDb.Any(s2 => s2.EmpId == s.EmpId));

            if (studentsExit.ToList().Count > 0)
            {
                await SaveStudentStudentCheckTime(studentsExit);
            }

            var studentsExitSentMsg = await MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);
            var result = studentsExitDbf.Where(s => !studentsExitSentMsg.Any(s2 => s2.EmpId == s.EmpId));

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

        public async Task SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType)
        {
            var results = new List<StudentSentMessage>();

            var url = GlobalConfig.AppSettings("sentMessageService")
                .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
                .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));

            foreach (var item in models)
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddBody("content-type", "application/form-data");

                //request.AddParameter("students", "999902");
                //request.AddParameter("message", $"ID: {item.EmpId} Name: {item.EmpName} {sentType.ToString()}: {item.ChkTime}");
                //request.AddParameter("rooms", "");
                //request.AddParameter("username", "0411");

                request.AddParameter("students", "999902");
                request.AddParameter("message", "test");
                request.AddParameter("rooms", "");
                request.AddParameter("username", "0411");

                await Task.Run(() =>
                {
                    client.ExecuteAsync(request, response =>
                    {

                        var data = response.Content;

                        StudentSentMessage model;
                        var json = response.Content;

                        ResponseMessage res = JsonConvert.DeserializeObject<ResponseMessage>(json);

                        if (res.success == "1")
                        {
                            model = new StudentSentMessage()
                            {
                                StudentCheckTimeId = item.Id,
                                Status = $"{SentStatus.Success}",
                                SentType = sentType.ToString(),
                                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:dd"))
                            };
                            results.Add(model);
                        }
                        else
                        {
                            model = new StudentSentMessage()
                            {
                                StudentCheckTimeId = item.Id,
                                Status = $"{SentStatus.Success}",
                                SentType = sentType.ToString(),
                                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:dd"))
                            };
                            results.Add(model);
                        }
                    });
                });
            }

            if(results.ToList().Count > 0)
            {
                await MySqlDataConnection.SaveStudentSentMessage(results);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            // get student entry from dbf file
            var studentsEntryDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student entry from MySql
            var studentsEntryDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsEntry = studentsEntryDbf.Where(s => !studentsEntryDb.Any(s2 => s2.EmpId == s.EmpId));

            return studentsEntry.StudentCheckTimesFirstTime();
        }

        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimesExit(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0) { return new List<StudentCheckTime>(); }

            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            // get student exit from dbf file
            var studentsExitDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student entry from MySql
            var studentsExitDb = await MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsExit = studentsExitDbf.Where(s => !studentsExitDb.Any(s2 => s2.EmpId == s.EmpId));

            return studentsExit.StudentCheckTimesFirstTime();
        }

        public async Task RemoveStudentsLess(string date)
        {
            await MySqlDataConnection.RemoveStudentsCheckTimeLess(date);
            await MySqlDataConnection.RemoveStudentsSentMessageLess(date);
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
            try
            {
                var url = GlobalConfig.AppSettings("sentMessageService")
                .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
                .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));

                var studentSentMessageDb = new List<StudentSentMessage>();


                if(sentType == SentType.Entry)
                {
                    studentSentMessageDb = GetStudentSentMessageEntryAsync(Helper.GetDate("current")).Result.ToList();
                    var entryError = studentSentMessageDb.Where(s => s.Status.ToLower() != "success");
                    var entrySuccess = studentSentMessageDb.Where(s => s.Status.ToLower() == "success");
                }
                else
                {
                    studentSentMessageDb = GetStudentSentMessageExitAsync(Helper.GetDate("current")).Result.ToList();
                    var exitError = studentSentMessageDb.Where(s => s.Status.ToLower() != "success");
                    var exitSuccess = studentSentMessageDb.Where(s => s.Status.ToLower() == "success");
                }

                var studentForSentMessage = models.Where(s => !studentSentMessageDb.Any(s2 => s.EmpId == s2.EmpId));

                foreach (var item in studentForSentMessage)
                {
                    var results = new List<StudentSentMessage>();
                    var client = new RestClient(url);
                    var request = new RestRequest(Method.POST);
                    request.AddBody("content-type", "application/form-data");

                    request.AddParameter("students", "999902");
                    request.AddParameter("message", $"ID: {item.EmpId} Name: {item.EmpName} {sentType.ToString()}: {item.ChkTime}");
                    request.AddParameter("rooms", "");
                    request.AddParameter("username", "0411");

                    // delay x sec
                    //Thread.Sleep(TimeSpan.FromSeconds(2));

                    await Task.Run(() =>
                    {
                        client.ExecuteAsync(request, async response =>
                        {

                            var data = response.Content;

                            StudentSentMessage model = new StudentSentMessage();
                            var json = response.Content;

                            ResponseMessage res = JsonConvert.DeserializeObject<ResponseMessage>(json);

                            if (res != null)
                            {
                                if (res.success == "1")
                                {
                                    model = new StudentSentMessage()
                                    {
                                        StudentCheckTimeId = item.Id,
                                        EmpId = item.EmpId,
                                        Status = $"{SentStatus.Success}",
                                        SentType = sentType.ToString(),
                                        SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss"))
                                    };
                                    results.Add(model);
                                }
                                else
                                {
                                    model = new StudentSentMessage()
                                    {
                                        StudentCheckTimeId = item.Id,
                                        EmpId = item.EmpId,

                                        Status = $"{SentStatus.Error} : {res.error}",
                                        SentType = sentType.ToString(),
                                        SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss"))
                                    };
                                    results.Add(model);
                                }

                                await MySqlDataConnection.SaveStudentSentMessage(results);
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                await SaveExceptionLog(ex);
            }
        }

        public async Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageEntryAsync(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0) { return new List<StudentSentMessage>(); }

            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student entry from dbf file
            var studentsEntryDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student sent message entry from MySql
            var studentsSentMessagesEntryDb = await MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsEntry = studentsEntryDbf.Where(s => !studentsSentMessagesEntryDb.Any(s2 => s2.EmpId == s.EmpId));

            var results = new List<StudentSentMessage>();

            return results.StudentStudentSentMessageFirstTime();
        }

        public async Task<IEnumerable<StudentSentMessage>> GetStudentSentMessageExitAsync(string date)
        {
            var data = await DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0) { return new List<StudentSentMessage>(); }

            var entryStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student entry from dbf file
            var studentsExitDbf = await GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student sent message entry from MySql
            var studentsSentMessagesExitDb = await MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsExit = studentsExitDbf.Where(s => !studentsSentMessagesExitDb.Any(s2 => s2.EmpId == s.EmpId));

            var results = new List<StudentSentMessage>();

            return results.StudentStudentSentMessageFirstTime();
        }

        public async Task SaveExceptionLog(Exception ex)
        {
            await MySqlDataConnection.SaveExceptionLog(ex);
        }
    }
}

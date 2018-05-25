using Newtonsoft.Json;
using Rajinibon.Common;
using Rajinibon.Models;
using Rajinibon.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rajinibon.DataAccess
{
    public class SentMessage
    {
        private static readonly IStudentService studentService = new StudentService();
        private static readonly IMySqlDataConnection mySqlDataConnection = new MySqlConnectors();
        private static volatile SentMessage _instance = null;
        private static Object _locker = new Object();
        public static SentMessage StudentSentMessage(IEnumerable<StudentCheckTime> models, SentType sentType)
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        try
                        {
                            var url = GlobalConfig.AppSettings("sentMessageService")
                            .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
                            .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));

                            var studentSentMessageDb = new List<StudentSentMessage>();

                            if (sentType == SentType.Entry)
                            {
                                studentSentMessageDb = studentService.GetStudentSentMessageEntryAsync(Helper.GetDate("current")).Result.ToList();
                            }
                            else if (sentType == SentType.Exit)
                            {
                                studentSentMessageDb = studentService.GetStudentSentMessageExitAsync(Helper.GetDate("current")).Result.ToList();
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
                                Thread.Sleep(TimeSpan.FromSeconds(int.Parse(GlobalConfig.AppSettings("ThreadSleepSentMessageSec"))));

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
                                                EmpId = item.EmpId,
                                                Status = $"{SentStatus.Success}",
                                                SentType = sentType.ToString(),
                                                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss"))
                                            };
                                            results.Add(model);
                                        }

                                        await mySqlDataConnection.SaveStudentSentMessage(results);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            studentService.SaveExceptionLog(ex).ConfigureAwait(false);
                        }
                        _instance = new SentMessage();
                    }
                }
            }
            return _instance;
        }

    }
}

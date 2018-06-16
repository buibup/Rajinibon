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

        public Tuple<List<StudentCheckTime>, List<StudentCheckTime>> GetStudentCheckTimesEntry(string date)
        {

            var data = DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0)
            {
                List<StudentCheckTime> item1 = new List<StudentCheckTime>();
                List<StudentCheckTime> item2 = new List<StudentCheckTime>();
                return Tuple.Create(item1, item2);
            }

            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student entry from dbf file
            var studentsEntryDbf = GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student entry from MySql
            var studentsEntryDb = MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsEntry = studentsEntryDbf.Where(s => !studentsEntryDb.Any(s2 => s2.EmpId == s.EmpId));

            return Tuple.Create(studentsEntry.StudentCheckTimesFirstTime(), studentsEntryDbf.StudentCheckTimesFirstTime());
        }

        public Tuple<List<StudentCheckTime>, List<StudentCheckTime>> GetStudentCheckTimesExit(string date)
        {
            var data = DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (data.ToList().Count == 0)
            {
                List<StudentCheckTime> item1 = new List<StudentCheckTime>();
                List<StudentCheckTime> item2 = new List<StudentCheckTime>();
                return Tuple.Create(item1, item2);
            }

            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            // get student exit from dbf file
            var studentsExitDbf = GetStudentCheckTimes(data, timeStart, timeEnd);

            // get student entry from MySql
            var studentsExitDb = MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            // get diff between dbf and mysql
            var studentsExit = studentsExitDbf.Where(s => !studentsExitDb.Any(s2 => s2.EmpId == s.EmpId));

            return Tuple.Create(studentsExit.StudentCheckTimesFirstTime(), studentsExitDbf.StudentCheckTimesFirstTime());
        }

        public void RemoveStudentsLess(string date)
        {
            MySqlDataConnection.RemoveStudentsCheckTimeLess(date);
            MySqlDataConnection.RemoveStudentsSentMessageLess(date);
        }

        public void SaveStudentStudentCheckTime(IEnumerable<StudentCheckTime> models)
        {
            MySqlDataConnection.SaveStudentCheckTimes(models);
        }

        public void SaveStudentSentMessage(IEnumerable<StudentCheckTime> models)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StudentCheckTime> GetStudentCheckTimes(IEnumerable<StudentCheckTime> models, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var result = new List<StudentCheckTime>();

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

            return result;
        }

        public void SentStudentNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType)
        {
            try
            {
                var url = GlobalConfig.AppSettings("sentMessageService")
                .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
                .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));


                var studentForSentMessage = new List<StudentCheckTime>();

                if (models.ToList().Count > 0)
                {
                    studentForSentMessage = models.ToList();
                }
                else
                {
                    if (sentType == SentType.Entry)
                    {
                        // get students checktime from databas
                        GlobalConfig.StudentCheckTimes = GetStudentsEntryMySql(GlobalConfig.Date);

                        // get students sent message from database
                        GlobalConfig.StudentSentMessages = GetStudentsSentMessagesEntryFromMySql(GlobalConfig.CurrentDate);

                        var diff = GlobalConfig.StudentCheckTimes.ToList().Count - GlobalConfig.StudentSentMessages.ToList().Count;

                        if (diff != 0)
                        {
                            // get students sent message error
                            var StudentSentMessagesError = GlobalConfig.StudentCheckTimes.Where(s => !GlobalConfig.StudentSentMessages.Any(s2 => s.EmpId == s2.EmpId));

                            if (StudentSentMessagesError.ToList().Count == 0) { return; }

                            SentStudentNotifyMessage(StudentSentMessagesError.Take(30), sentType);
                        }
                    }
                    else if (sentType == SentType.Exit)
                    {
                        // get students checktime from databas
                        GlobalConfig.StudentCheckTimes = GetStudentsExitMySql(GlobalConfig.Date);

                        // get students sent message from database
                        GlobalConfig.StudentSentMessages = GetStudentsSentMessagesExitFromMySql(GlobalConfig.CurrentDate);

                        var diff = GlobalConfig.StudentCheckTimes.ToList().Count - GlobalConfig.StudentSentMessages.ToList().Count;

                        if (diff != 0)
                        {
                            // get students sent message error
                            var StudentSentMessagesError = GlobalConfig.StudentCheckTimes.Where(s => !GlobalConfig.StudentSentMessages.Any(s2 => s.EmpId == s2.EmpId));

                            if (StudentSentMessagesError.ToList().Count == 0) { return; }

                            SentStudentNotifyMessage(StudentSentMessagesError.Take(30), sentType);
                        }
                    }

                    return;
                }

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
                    //Thread.Sleep(TimeSpan.FromSeconds(int.Parse(GlobalConfig.AppSettings("ThreadSleepSentMessageSec"))));
                    Thread.Sleep(100);

                    client.ExecuteAsync(request, response =>
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
                                //GlobalConfig.StudentSentMessages.Add(model);


                                MySqlDataConnection.SaveStudentSentMessage(results);
                            }
                            else
                            {
                                model = new StudentSentMessage()
                                {
                                    EmpId = item.EmpId,
                                    Status = $"{SentStatus.Error} : {res.error}",
                                    SentType = sentType.ToString(),
                                    SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss"))
                                };
                                //GlobalConfig.StudentSentMessages.Add(model);
                            }
                        }
                        else
                        {
                            model = new StudentSentMessage()
                            {
                                EmpId = item.EmpId,
                                Status = $"{SentStatus.Error}",
                                SentType = sentType.ToString(),
                                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss"))
                            };
                            //GlobalConfig.StudentSentMessages.Add(model);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
            }
        }

        public IEnumerable<StudentSentMessage> GetStudentSentMessageEntry(string date)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student sent message entry from MySql
            var studentsSentMessagesEntryDb = MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);

            return studentsSentMessagesEntryDb;
        }

        public IEnumerable<StudentSentMessage> GetStudentSentMessageExit(string date)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            // get student sent message entry from MySql
            var studentsSentMessagesExitDb = MySqlDataConnection.GetStudentSentMessages(date.GetDate(), timeStart, timeEnd);

            return studentsSentMessagesExitDb;
        }

        public void SaveExceptionLog(Exception ex)
        {
            MySqlDataConnection.SaveExceptionLog(ex);
        }

        public bool SentMessageSuccess(IEnumerable<StudentCheckTime> models, SentType sentType)
        {
            throw new NotImplementedException();
        }

        public List<StudentCheckTime> GetStudentsEntryDbf(string date)
        {
            var data = DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (date.ToList().Count == 0) { return new List<StudentCheckTime>(); }

            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student entry from dbf file
            var studentsEntryDbf = GetStudentCheckTimes(data, timeStart, timeEnd);

            return studentsEntryDbf.StudentCheckTimesFirstTime();
        }

        public List<StudentCheckTime> GetStudentsExitDbf(string date)
        {
            var data = DbfDataConnection.GetStudentCheckTimes(GlobalConfig.DbfPath, date);

            if (date.ToList().Count == 0) { return new List<StudentCheckTime>(); }

            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            // get student exit from dbf file
            var studentsExitDbf = GetStudentCheckTimes(data, timeStart, timeEnd);

            return studentsExitDbf.StudentCheckTimesFirstTime();
        }

        public List<StudentCheckTime> GetStudentsEntryMySql(string date)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            var result = MySqlDataConnection.GetStudentCheckTimes(date, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentCheckTime> GetStudentsExitMySql(string date)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            var result = MySqlDataConnection.GetStudentCheckTimes(date, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentCheckTime> GetStudentsEntryFromList(List<StudentCheckTime> models)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            var result = GetStudentCheckTimes(models, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentCheckTime> GetStudentsExitFromList(List<StudentCheckTime> models)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            var result = GetStudentCheckTimes(models, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentSentMessage> GetStudentSentMessageEntryFromList(List<StudentSentMessage> models)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            var result = GetStudentSentMessage(models, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentSentMessage> GetStudentSentMessageExitFromList(List<StudentSentMessage> models)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            var result = GetStudentSentMessage(models, timeStart, timeEnd).ToList();

            return result;
        }

        public IEnumerable<StudentSentMessage> GetStudentSentMessage(IEnumerable<StudentSentMessage> models, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var result = new List<StudentSentMessage>();

            foreach (var item in models)
            {
                var sentTime = item.SentTime;
                var time = new TimeSpan(sentTime.Hour, sentTime.Minute, sentTime.Millisecond);

                if (time.IsBetween(timeStart, timeEnd))
                {
                    var model = new StudentSentMessage()
                    {
                        EmpId = item.EmpId,
                        Status = item.Status,
                        SentType = item.SentType,
                        SentTime = sentTime
                    };
                    result.Add(model);
                }
            }

            return result;
        }

        public List<StudentSentMessage> GetStudentsSentMessagesEntryFromMySql(string date)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            var result = MySqlDataConnection.GetStudentSentMessages(date, timeStart, timeEnd).ToList();

            return result;
        }

        public List<StudentSentMessage> GetStudentsSentMessagesExitFromMySql(string date)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            var result = MySqlDataConnection.GetStudentSentMessages(date, timeStart, timeEnd).ToList();

            return result;
        }

        public ResponseMessage SentOnceNotifyMessage(StudentCheckTime model, SentType sentType)
        {
            var result = new ResponseMessage();
            if (MySqlDataConnection.SentSuccess(model.EmpId, sentType))
            {
                result = new ResponseMessage()
                {
                    success = "1"
                };
                return result;
            }

            var studentsAddPara = GlobalConfig.AppSettings("students");
            var roomsAddPara = GlobalConfig.AppSettings("rooms");
            var messageAddPara = GlobalConfig.AppSettings("message");
            var usernameAddPara = GlobalConfig.AppSettings("username");

            var url = GlobalConfig.AppSettings("sentMessageService")
            .Replace("{schoolCode}", GlobalConfig.AppSettings("schoolCode"))
            .Replace("{roleCode}", GlobalConfig.AppSettings("roleCode"));

            var studentsReq = studentsAddPara == "999902" ? studentsAddPara : model.EmpId;
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddBody("content-type", "application/form-data");

            #region message
            //entry
            //ทดสอบ
            //รหัสนักเรียน
            //ชื่อ
            //เวลาเข้าเรียน

            //exit
            //ทดสอบ
            //รหัสนักเรียน
            //ชื่อ
            //เวลาเลิกเรียน
            #endregion

            request.AddParameter("students", studentsReq);
            if (sentType == SentType.Entry)
            {
                request.AddParameter("message",
                $"ทดสอบ {GlobalConfig.StudentCount}" + System.Environment.NewLine +
                $"รหัสนักเรียน: {model.EmpId}" + System.Environment.NewLine +
                $"ชื่อ: {model.EmpName}" + System.Environment.NewLine +
                $"เวลาเข้าเรียน: {model.ChkTime}");
            }
            else if (sentType == SentType.Exit)
            {
                request.AddParameter("message",
                $"ทดสอบ {GlobalConfig.StudentCount}" + System.Environment.NewLine +
                $"รหัสนักเรียน: {model.EmpId}" + System.Environment.NewLine +
                $"ชื่อ: {model.EmpName}" + System.Environment.NewLine +
                $"เวลาเลิกเรียน: {model.ChkTime}");
            }

            request.AddParameter("rooms", roomsAddPara);
            request.AddParameter("username", usernameAddPara);

            StudentSentMessage sentMessage = new StudentSentMessage();

            var json = client.Execute<ResponseMessage>(request).Content;
            result = JsonConvert.DeserializeObject<ResponseMessage>(json);

            //if (res != null)
            //{
            if (result.success == "1")
            {
                GlobalConfig.StudentCount += 1;
                sentMessage = new StudentSentMessage()
                {
                    EmpId = model.EmpId,
                    Status = $"{SentStatus.Success}",
                    SentType = sentType.ToString(),
                    SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss")),
                    ChkTime = model.ChkTime
                };

                MySqlDataConnection.SaveStudentSentMessage(sentMessage);
            }
            else
            {
                sentMessage = new StudentSentMessage()
                {
                    EmpId = model.EmpId,
                    Status = $"{SentStatus.Error} : {result.error}",
                    SentType = sentType.ToString(),
                    SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss")),
                    ChkTime = model.ChkTime
                };
                MySqlDataConnection.SaveStudentSentMessage(sentMessage);
            }
            //}

            #region sent Async

            //client.ExecuteAsync(request, response =>
            //{
            //    StudentSentMessage sentMessage = new StudentSentMessage();
            //    var json = response.Content;

            //    ResponseMessage resAsync = JsonConvert.DeserializeObject<ResponseMessage>(json);

            //    result = resAsync;

            //    if (resAsync != null)
            //    {
            //        if (resAsync.success == "1")
            //        {
            //            sentMessage = new StudentSentMessage()
            //            {
            //                EmpId = model.EmpId,
            //                Status = $"{SentStatus.Success}",
            //                SentType = sentType.ToString(),
            //                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss")),
            //                ChkTime = model.ChkTime
            //            };

            //            MySqlDataConnection.SaveStudentSentMessage(sentMessage);
            //        }
            //        else
            //        {
            //            sentMessage = new StudentSentMessage()
            //            {
            //                EmpId = model.EmpId,
            //                Status = $"{SentStatus.Error} : {resAsync.error}",
            //                SentType = sentType.ToString(),
            //                SentTime = DateTime.Parse(Helper.GetDateNowStringUs("yyyy-MM-dd HH:mm:ss")),
            //                ChkTime = model.ChkTime
            //            };
            //            MySqlDataConnection.SaveStudentSentMessage(sentMessage);
            //        }
            //    }
            //});
            #endregion

            return result;
        }

        public void SentStudentsNotifyMessage(IEnumerable<StudentCheckTime> models, SentType sentType)
        {
            try
            {
                foreach (var item in models)
                {
                    if (MySqlDataConnection.SentSuccess(item.EmpId, sentType))
                    {
                        continue;
                    }

                    var responseMsg = SentOnceNotifyMessage(item, sentType);
                    while (responseMsg.success != "1")
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(double.Parse(GlobalConfig.AppSettings("ThreadSleepSentMessageSec"))));
                        responseMsg = SentOnceNotifyMessage(item, sentType);
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(double.Parse(GlobalConfig.AppSettings("ThreadSleepSentMessageSec"))));
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
            }

        }

        public List<StudentCheckTime> GetStudentCheckTimesEntryMySql(string date)
        {
            var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
            var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

            // get student entry from MySql
            var studentsEntryDb = MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            return studentsEntryDb.ToList();
        }

        public List<StudentCheckTime> GetStudentCheckTimesExitMySql(string date)
        {
            var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
            var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

            var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
            var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

            // get student entry from MySql
            var studentsExitDb = MySqlDataConnection.GetStudentCheckTimes(date.GetDate(), timeStart, timeEnd);

            return studentsExitDb.ToList();
        }

        public void RemoveSentMessageError()
        {
            MySqlDataConnection.RemoveSentMessageError();
        }

        public List<StudentSentMessage> GetStudentsSentMessageError()
        {
            var results = MySqlDataConnection.GetStudentsSentMessageError();

            return results;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Rajinibon.Common;
using Rajinibon.Models;

namespace Rajinibon.DataAccess
{
    public class MySqlConnectors : IMySqlDataConnection
    {
        private readonly string connString = GlobalConfig.CnnString("DefaultConnection");

        public IEnumerable<StudentCheckTime> GetStudentCheckTimes(string date, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentCheckTime>();
            using(var connection = new MySqlConnection(connString))
            {
                results = connection.QueryAsync<StudentCheckTime>(MySqlDbQuery.GetStudentCheckTimeByDate(), new { chk_time = date }).Result.ToList();
            }

            return results.GetStudentCheckTimes(timeStart, timeEnd);
        }

        public IEnumerable<StudentSentMessage> GetStudentSentMessages(string date, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentSentMessage>();
            using (var connection = new MySqlConnection(connString))
            {
                results = connection.QueryAsync<StudentSentMessage>(MySqlDbQuery.GetStudentSentMessagesByDate(), new { chk_time = date }).Result.ToList();
            }

            return results.GetStudentSentMessage(timeStart, timeEnd);
        }

        public List<StudentSentMessage> GetStudentsSentMessageError()
        {
            var results = new List<StudentSentMessage>();
            using (var connection = new MySqlConnection(connString))
            {
                results = connection.QueryAsync<StudentSentMessage>(MySqlDbQuery.GetStudentsSentMessageError()).Result.ToList();

                return results;
            }
        }

        public void RemoveSentMessageError()
        {
            using (var connection = new MySqlConnection(connString))
            {
                connection.QueryAsync(MySqlDbQuery.RemoveSentMessageError());
            }
        }

        public void RemoveStudentsCheckTimeLess(string date)
        {
            using(var connection = new MySqlConnection(connString))
            {
                connection.Query(MySqlDbQuery.RemoveStudentsCheckTimeLess(), new { chk_time = date });
            }
        }

        public void RemoveStudentsSentMessageLess(string date)
        {
            using (var connection = new MySqlConnection(connString))
            {
                connection.Query(MySqlDbQuery.RemoveStudentsSentMessageLess(), new { chk_time = date });
            }
        }

        public void SaveExceptionLog(Exception ex)
        {
            var model = new ExceptionLog()
            {
                Message = ex.Message.ToString(),
                StackTrace = ex.StackTrace.ToString()
            };
            using(var connection = new MySqlConnection(connString))
            {
                connection.Execute(MySqlDbQuery.SaveExceptionLog(), ex);
            }
        }

        public void SaveStudentCheckTimes(IEnumerable<StudentCheckTime> models)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Execute(MySqlDbQuery.SaveStudentCheckTimes(), models);
                }
            }
            catch (Exception)
            {
            }
        }

        public void SaveStudentSentMessage(IEnumerable<StudentSentMessage> models)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Execute(MySqlDbQuery.SaveStudentSentMessages(), models);
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
            }
        }

        public void SaveStudentSentMessage(StudentSentMessage model)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Execute(MySqlDbQuery.SaveStudentSentMessages(), model);
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
            }
        }

        public void SaveStudentSentMessageAsync(IEnumerable<StudentSentMessage> models)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    connection.ExecuteAsync(MySqlDbQuery.SaveStudentSentMessages(), models);
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
            }
        }

        public bool SentSuccess(string empId, SentType sentType)
        {
            bool result = false;
            try
            {
                using(var connection = new MySqlConnection(connString))
                {
                    var item = connection.Query<StudentSentMessage>(MySqlDbQuery.SentSuccess(), new { emp_id = empId }).ToList();
                    var model = new StudentSentMessage();

                    if(sentType == SentType.Entry)
                    {
                        var entryStartTime = GlobalConfig.AppSettings("entryStartTime").Split(':');
                        var entryEndTime = GlobalConfig.AppSettings("entryEndTime").Split(':');

                        var timeStart = new TimeSpan(int.Parse(entryStartTime[0]), int.Parse(entryStartTime[1]), int.Parse(entryStartTime[2]));
                        var timeEnd = new TimeSpan(int.Parse(entryEndTime[0]), int.Parse(entryEndTime[1]), int.Parse(entryEndTime[2]));

                        model = item.GetStudentSentMessage(timeStart, timeEnd).FirstOrDefault();
                    }
                    else if(sentType == SentType.Exit)
                    {
                        var exitStartTime = GlobalConfig.AppSettings("exitStartTime").Split(':');
                        var exitEndTime = GlobalConfig.AppSettings("exitEndTime").Split(':');

                        var timeStart = new TimeSpan(int.Parse(exitStartTime[0]), int.Parse(exitStartTime[1]), int.Parse(exitStartTime[2]));
                        var timeEnd = new TimeSpan(int.Parse(exitEndTime[0]), int.Parse(exitEndTime[1]), int.Parse(exitEndTime[2]));

                        model = item.GetStudentSentMessage(timeStart, timeEnd).FirstOrDefault();
                    }
                    if(model != null)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                SaveExceptionLog(ex);
                result = false;
            }

            return result;
        }
    }
}

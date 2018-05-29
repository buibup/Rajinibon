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

        public async Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(string date, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentCheckTime>();
            using(var connection = new MySqlConnection(connString))
            {
                await connection.OpenAsync();
                results = connection.QueryAsync<StudentCheckTime>(MySqlDbQuery.GetStudentCheckTimeByDate(), new { chk_time = date }).Result.ToList();
            }

            return results.GetStudentCheckTimes(timeStart, timeEnd);
        }

        public async Task<IEnumerable<StudentSentMessage>> GetStudentSentMessages(string date, TimeSpan timeStart, TimeSpan timeEnd)
        {
            var results = new List<StudentSentMessage>();
            using (var connection = new MySqlConnection(connString))
            {
                await connection.OpenAsync();
                results = connection.QueryAsync<StudentSentMessage>(MySqlDbQuery.GetStudentSentMessagesByDate(), new { sent_time = date }).Result.ToList();
            }

            if (bool.Parse(GlobalConfig.AppSettings("Test"))) { return results; }

            return results.GetStudentSentMessage(timeStart, timeEnd);
        }

        public async Task<List<StudentSentMessage>> GetStudentsSentMessageError()
        {
            var results = new List<StudentSentMessage>();
            using (var connection = new MySqlConnection(connString))
            {
                await connection.OpenAsync();
                results = connection.QueryAsync<StudentSentMessage>(MySqlDbQuery.GetStudentsSentMessageError()).Result.ToList();

                return results;
            }
        }

        public async Task RemoveSentMessageError()
        {
            using (var connection = new MySqlConnection(connString))
            {
                await connection.QueryAsync(MySqlDbQuery.RemoveSentMessageError());
            }
        }

        public async Task RemoveStudentsCheckTimeLess(string date)
        {
            using(var connection = new MySqlConnection(connString))
            {
                await connection.QueryAsync(MySqlDbQuery.RemoveStudentsCheckTimeLess(), new { chk_time = date });
            }
        }

        public async Task RemoveStudentsSentMessageLess(string date)
        {
            using (var connection = new MySqlConnection(connString))
            {
                await connection.QueryAsync(MySqlDbQuery.RemoveStudentsSentMessageLess(), new { sent_time = date });
            }
        }

        public async Task SaveExceptionLog(Exception ex)
        {
            var model = new ExceptionLog()
            {
                Message = ex.Message.ToString(),
                StackTrace = ex.StackTrace.ToString()
            };
            using(var connection = new MySqlConnection(connString))
            {
                await connection.ExecuteAsync(MySqlDbQuery.SaveExceptionLog(), ex);
            }
        }

        public async Task SaveStudentCheckTimes(IEnumerable<StudentCheckTime> models)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    await connection.ExecuteAsync(MySqlDbQuery.SaveStudentCheckTimes(), models);
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
                SaveExceptionLog(ex).ConfigureAwait(false);
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
                SaveExceptionLog(ex).ConfigureAwait(false);
            }
        }

        public async Task SaveStudentSentMessageAsync(IEnumerable<StudentSentMessage> models)
        {
            try
            {
                using (var connection = new MySqlConnection(connString))
                {
                    await connection.ExecuteAsync(MySqlDbQuery.SaveStudentSentMessages(), models);
                }
            }
            catch (Exception ex)
            {
                await SaveExceptionLog(ex);
            }
        }
    }
}

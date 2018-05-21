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
    }
}

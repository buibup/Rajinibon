using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDbfReader;
using Rajinibon.Common;
using Rajinibon.Models;

namespace Rajinibon.DataAccess
{
    public class DbfConnector : IDbfDataConnection
    {
        public IEnumerable<StudentCheckTime> GetStudentCheckTimes(string rootPath, string date)
        {
            var results = new List<StudentCheckTime>();
            var fullPath = rootPath.GetFullPath(date);

            if (File.Exists(fullPath))
            {
                // Protect Exception Another Process
                var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using (var table = Table.Open(fs))
                {
                    var reader = table.OpenReader(Encoding.GetEncoding(874));
                    while (reader.Read())
                    {
                        decimal? cuserid = reader.GetDecimal("CUSERID") == null ? 0 : reader.GetDecimal("CUSERID");
                        DateTime? chkTime = reader.GetDateTime("CHKTIME");

                        var model = new StudentCheckTime()
                        {
                            CuserId = cuserid,
                            EmpId = reader.GetString("EMPID"),
                            EmpName = reader.GetString("EMPNAME"),
                            ChkTime = (DateTime)chkTime
                        };

                        results.Add(model);
                    }
                }
            }

            return results;
        }

        public Task<IEnumerable<StudentCheckTime>> GetStudentCheckTimes(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            throw new NotImplementedException();
        }
    }
}

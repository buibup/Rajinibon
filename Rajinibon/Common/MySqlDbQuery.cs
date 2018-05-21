﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon.Common
{
    public static class MySqlDbQuery
    {
        public static string GetStudentCheckTimeByDate()
        {
            return @"
                SELECT cuser_id CuserId, emp_id EmpId,emp_name EmpName, chk_time ChkTime
                FROM students_check_time
                where date(chk_time) = ?
            ";
        }

        public static string SaveStudentCheckTimes()
        {
            return @"
                INSERT INTO students_check_time
                (cuser_id, emp_id, emp_name, chk_time)
                VALUES(?CuserId, ?EmpId, ?EmpName, ?ChkTime);
            ";
        }
    }
}

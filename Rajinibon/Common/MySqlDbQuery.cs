using System;
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
                SELECT id Id, cuser_id CuserId, emp_id EmpId,emp_name EmpName, chk_time ChkTime
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

        public static string GetStudentSentMessagesByDate()
        {
            return @"
                SELECT id Id, emp_id EmpId, sent_type SentType, status Status, sent_time SentTime, 
                status Status, sent_time SentTime
                FROM students_sent_message
                where date(sent_time) = ?
            ";
        }

        public static string GetStudentsSentMessageError()
        {
            return @"
                select id Id, emp_id EmpId, sent_type SentType, status Status, sent_time SentTime, 
                status Status, sent_time SentTime
                from students_sent_message
                where lower(status) <> 'success'
            ";
        }

        public static string RemoveSentMessageError()
        {
            return @"
                DELETE FROM students_sent_message
                where lower(status) <> 'success'";
        }

       
        public static string SaveStudentSentMessages()
        {
            return @"
                INSERT INTO rajinibon.students_sent_message
                (emp_id, sent_type, status, sent_time)
                VALUES(?EmpId, ?SentType, ?Status, ?SentTime);
            ";
        }

        public static string SaveExceptionLog()
        {
            return @"
                INSERT INTO exception_logs
                (message, stack_trace)
                VALUES(?Message, ?StackTrace);
            ";
        }

        public static string RemoveStudentsCheckTimeLess()
        {
            return @"
                DELETE FROM students_check_time
                WHERE date(chk_time) < ?
            ";
        }

        public static string RemoveStudentsSentMessageLess()
        {
            return @"
                DELETE FROM students_sent_message
                WHERE date(sent_time) < ?
            ";
        }
    }
}

using Rajinibon.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rajinibon.SentMessage
{
    class Program
    {
        private static IStudentService studentService;
        static void Main(string[] args)
        {
            studentService = new StudentService();

            var timeStartConfig = GlobalConfig.AppSettings("taskStartTime").Split(':');
            var timeEndConfig = GlobalConfig.AppSettings("taskEndTime").Split(':');

            var startTime = new TimeSpan(int.Parse(timeStartConfig[0]), int.Parse(timeStartConfig[1]), int.Parse(timeStartConfig[2])); //DateTime.Now.TimeOfDay;
            var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

            Console.WriteLine($"Task sent message proccessing...");

            //sent message
            RunSentMessage(startTime, endTime);

            Console.WriteLine($"Task sent message finish.");
            Thread.Sleep(5000);
            Environment.Exit(0);
            
        }
        //private static System.Threading.Timer timer;
        private static void RunSentMessage(TimeSpan startTime, TimeSpan endTime)
        {
            
            var currentTime = DateTime.Now.TimeOfDay;
    
            // startTime and currentTime <= endTime
            while (currentTime >= startTime && currentTime <= endTime)
            {
                try
                {

                    #region 1.ดึงข้อมูลนักเรียนที่ไม่เคยส่งข้อความในช่วงเวลานั้นๆ
                    // get all student
                    var studentsCheckTimeEntry = studentService.GetStudentCheckTimesEntryMySql(GlobalConfig.Date).ToList();
                    var studentsCheckTimeExit = studentService.GetStudentCheckTimesExitMySql(GlobalConfig.Date).ToList();

                    // get all student sent message
                    var studentsSentMessageEntry = studentService.GetStudentSentMessageEntry(GlobalConfig.CurrentDate).ToList();
                    var studentsSentMessageExit = studentService.GetStudentSentMessageExit(GlobalConfig.CurrentDate).ToList();

                    // get all student sent message and success
                    var studentsSuccessEntry = studentsSentMessageEntry.Where(std => std.Status.ToLower() == "success").ToList();
                    var studentsSuccessExit = studentsSentMessageExit.Where(std => std.Status.ToLower() == "success").ToList();

                    // get student for sent message
                    var studentsForSentMsgEntry = studentsCheckTimeEntry.Where(std => !studentsSuccessEntry.Any(std2 => std.EmpId == std2.EmpId)).ToList();
                    var studentsForSentMsgExit = studentsCheckTimeExit.Where(std => !studentsSuccessExit.Any(std2 => std.EmpId == std2.EmpId)).ToList();
                    #endregion

                    if (studentsForSentMsgEntry.Count > 0)
                    {
                        // sent message entry
                        studentService.SentStudentsNotifyMessage(studentsForSentMsgEntry, SentType.Entry);
                    }

                    if (studentsForSentMsgExit.Count > 0)
                    {
                        // sent message exit
                        studentService.SentStudentsNotifyMessage(studentsForSentMsgEntry, SentType.Exit);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(2));

                }
                catch (Exception exApp)
                {
                    // show exception on console
                    Console.WriteLine($"{DateTime.Now}: {exApp.Message.ToString()}");
                }
                currentTime = DateTime.Now.TimeOfDay;
            }
        }
    }
}

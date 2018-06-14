using Rajinibon.Common;
using Rajinibon.Models;
using Rajinibon.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Rajinibon.Task
{
    class Program
    {
        private static IStudentService studentService;

        static void Main(string[] args)
        {
            studentService = new StudentService();

            // remove student < get date
            studentService.RemoveStudentsLess(GlobalConfig.Date.GetDate()).ConfigureAwait(false);

            var timeStartConfig = GlobalConfig.AppSettings("taskStartTime").Split(':');
            var timeEndConfig = GlobalConfig.AppSettings("taskEndTime").Split(':');

            var startTime = DateTime.Now.TimeOfDay;  //new TimeSpan(int.Parse(timeStartConfig[0]), int.Parse(timeStartConfig[1]), int.Parse(timeStartConfig[2])); 
            var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

            // save check time
            RunTask(startTime, endTime);

            //sent message
            //RunSentMessage(startTime, endTime);

            Console.WriteLine($"Task save check time proccessing...");
            Console.ReadLine();
        }

        private static System.Threading.Timer timer;
        private static System.Threading.Timer timerSentMessage;
        static void RunTask(TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                DateTime current = DateTime.Now;
                TimeSpan timeToGo = DateTime.Now.TimeOfDay - current.TimeOfDay;
                if (timeToGo < TimeSpan.Zero)
                {
                    return;//time already passed
                }
                timer = new System.Threading.Timer(x =>
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();

                    while (startTime + s.Elapsed <= endTime)
                    {
                        var studentsEntry = studentService.GetStudentCheckTimesEntry(GlobalConfig.Date).Result;
                        var studentsExit = studentService.GetStudentCheckTimesExit(GlobalConfig.Date).Result;
                        RunStudentsCheckTime(studentsEntry.Item1, studentsExit.Item1, SentMethod.New);

                        #region sent message
                        //var studentsCheckTimeEntry = studentService.GetStudentCheckTimesEntryMySql(GlobalConfig.Date).Result.ToList();
                        //var studentsCheckTimeExit = studentService.GetStudentCheckTimesExitMySql(GlobalConfig.Date).Result.ToList();
                        //var studentsSentMessageEntry = studentService.GetStudentSentMessageEntryAsync(GlobalConfig.CurrentDate).Result.ToList();
                        //var studentsSentMessageExit = studentService.GetStudentSentMessageExitAsync(GlobalConfig.CurrentDate).Result.ToList();
                        //SentMessageError(studentsCheckTimeEntry, 
                        //    studentsCheckTimeExit, 
                        //    studentsSentMessageEntry.Where(std => std.Status.ToLower() == "success").ToList(), 
                        //    studentsSentMessageExit.Where(std => std.Status.ToLower() == "success").ToList());
                        #endregion

                        // sleep of thread
                        Thread.Sleep(TimeSpan.FromSeconds(double.Parse(GlobalConfig.AppSettings("ThreadSleepTaskSec"))));
                    }
                    s.Stop();
                    Environment.Exit(0);
                }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                studentService.SaveExceptionLog(ex);
            }
        }

        private static void RunStudentsCheckTime(
            List<StudentCheckTime> studentCheckTimesEntry, 
            List<StudentCheckTime> studentCheckTimesExit,
            SentMethod sentMethod)
        {
            if(studentCheckTimesEntry.Count > 0)
            {
                if(sentMethod == SentMethod.New)
                {
                    // save entry
                    studentService.SaveStudentStudentCheckTime(studentCheckTimesEntry);
                }

                // sent message entry
                //studentService.SentStudentsNotifyMessage(studentCheckTimesEntry, SentType.Entry);
            }
            if (studentCheckTimesExit.Count > 0)
            {
                if (sentMethod == SentMethod.New)
                {
                    // save exit
                    studentService.SaveStudentStudentCheckTime(studentCheckTimesExit);
                }

                // sent message exit
                //studentService.SentStudentsNotifyMessage(studentCheckTimesExit, SentType.Exit);
            }
        }

        private static void SentMessageError(
            List<StudentCheckTime> studentCheckTimesEntry,
            List<StudentCheckTime> studentCheckTimesExit,
            List<StudentSentMessage> studentSentMessagesEntry,
            List<StudentSentMessage> studentSentMessagesExit)
        {

            if(studentCheckTimesEntry.Count != studentSentMessagesEntry.Count)
            {
                var studentsSentMessageEntry = studentCheckTimesEntry.Where(s => !studentSentMessagesEntry.Any(s2 => s.EmpId == s2.EmpId)).ToList();

                RunStudentsCheckTime(studentsSentMessageEntry, new List<StudentCheckTime>(), SentMethod.Error);
            }

            if (studentCheckTimesExit.Count != studentSentMessagesExit.Count)
            {
                var studentsSentMessageExit = studentCheckTimesExit.Where(s => !studentSentMessagesExit.Any(s2 => s.EmpId == s2.EmpId)).ToList();

                RunStudentsCheckTime(new List<StudentCheckTime>(), studentsSentMessageExit, SentMethod.Error);
            }
        }

        private static void RunSentMessage(TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                DateTime current = DateTime.Now;
                TimeSpan timeToGo = DateTime.Now.TimeOfDay - current.TimeOfDay;
                if (timeToGo < TimeSpan.Zero)
                {
                    return;//time already passed
                }
                timerSentMessage = new System.Threading.Timer(x =>
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();

                    while (startTime + s.Elapsed <= endTime)
                    {
                        double sentTimeEntry = 0;
                        double sentTimeExit = 0;

                        /** Process sent student message
                         * 1. get all student check time 
                         * 2. loop sent each student every 2 sec (count students * 2 then use for sleep thead)
                         * 3. check error students
                         * 4. sent error students
                        */

                        #region 1.ดึงข้อมูลนักเรียนที่ไม่เคยส่งข้อความในช่วงเวลานั้นๆ
                        // get all student
                        var studentsCheckTimeEntry = studentService.GetStudentCheckTimesEntryMySql(GlobalConfig.Date).Result.ToList();
                        var studentsCheckTimeExit = studentService.GetStudentCheckTimesExitMySql(GlobalConfig.Date).Result.ToList();

                        // get all student sent message
                        var studentsSentMessageEntry = studentService.GetStudentSentMessageEntryAsync(GlobalConfig.CurrentDate).Result.ToList();
                        var studentsSentMessageExit = studentService.GetStudentSentMessageExitAsync(GlobalConfig.CurrentDate).Result.ToList();

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
                            sentTimeEntry = studentsForSentMsgEntry.Count * 2;
                            Thread.Sleep(TimeSpan.FromSeconds(sentTimeEntry));
                        }

                        if (studentsForSentMsgExit.Count > 0)
                        {
                            // sent message exit
                            studentService.SentStudentsNotifyMessage(studentsForSentMsgEntry, SentType.Exit);
                            sentTimeExit = studentsForSentMsgExit.Count * 2;
                            Thread.Sleep(TimeSpan.FromSeconds(sentTimeExit));
                        }

                        //.Sleep(TimeSpan.FromSeconds(double.Parse(GlobalConfig.AppSettings("ThreadSleepSentMessageSec"))));
                    }
                    s.Stop();
                    Environment.Exit(0);
                }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                studentService.SaveExceptionLog(ex);
            }
        }
    }
}

﻿using Rajinibon.Common;
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

            var timeEndConfig = GlobalConfig.AppSettings("taskEndTime").Split(':');

            var startTime = DateTime.Now.TimeOfDay;
            var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

            RunTask(startTime, endTime);
            Console.WriteLine($"Task sent message proccessing...");
            Console.ReadLine();
        }

        private static System.Threading.Timer timer;
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

                        var studentsCheckTimeEntry = studentService.GetStudentCheckTimesEntryMySql(GlobalConfig.Date).Result.ToList();
                        var studentsCheckTimeExit = studentService.GetStudentCheckTimesExitMySql(GlobalConfig.Date).Result.ToList();
                        var studentsSentMessageEntry = studentService.GetStudentSentMessageEntryAsync(GlobalConfig.CurrentDate).Result.ToList();
                        var studentsSentMessageExit = studentService.GetStudentSentMessageExitAsync(GlobalConfig.CurrentDate).Result.ToList();
                        SentMessageError(studentsCheckTimeEntry, studentsCheckTimeExit, studentsSentMessageEntry, studentsSentMessageExit);

                        // delay of task
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
                studentService.SentStudentsNotifyMessage(studentCheckTimesEntry, SentType.Entry);
            }
            if (studentCheckTimesExit.Count > 0)
            {
                if (sentMethod == SentMethod.New)
                {
                    // save exit
                    studentService.SaveStudentStudentCheckTime(studentCheckTimesExit);
                }

                // sent message exit
                studentService.SentStudentsNotifyMessage(studentCheckTimesExit, SentType.Entry);
            }
        }

        private static void SentMessageError(
            List<StudentCheckTime> studentCheckTimesEntry,
            List<StudentCheckTime> studentCheckTimesExit,
            List<StudentSentMessage> studentSentMessagesEntry,
            List<StudentSentMessage> studentSentMessagesExit)
        {
            if(studentCheckTimesEntry.Count == 0 && studentCheckTimesExit.Count == 0) { return; }

            if(studentCheckTimesEntry.Count == studentSentMessagesEntry.Count)
            {
                var studentsSentMessageError = studentService.GetStudentsSentMessageError().Result.ToList();
                studentService.RemoveSentMessageError();

                var studentsSentMessage = studentCheckTimesEntry.Where(s => studentsSentMessageError.Any(s2 => s.EmpId == s2.EmpId)).ToList();

                RunStudentsCheckTime(studentsSentMessage, new List<StudentCheckTime>(), SentMethod.Error);
            }

            if (studentCheckTimesExit.Count == studentSentMessagesExit.Count)
            {
                var studentsSentMessageError = studentService.GetStudentsSentMessageError().Result.ToList();
                studentService.RemoveSentMessageError();

                var studentsSentMessage = studentCheckTimesExit.Where(s => studentsSentMessageError.Any(s2 => s.EmpId == s2.EmpId)).ToList();

                RunStudentsCheckTime(new List<StudentCheckTime>(), studentsSentMessage, SentMethod.Error);
            }
        }
    }
}

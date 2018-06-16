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
            studentService.RemoveStudentsLess(GlobalConfig.Date.GetDate());

            var timeStartConfig = GlobalConfig.AppSettings("taskStartTime").Split(':');
            var timeEndConfig = GlobalConfig.AppSettings("taskEndTime").Split(':');

            var startTime = new TimeSpan(int.Parse(timeStartConfig[0]), int.Parse(timeStartConfig[1]), int.Parse(timeStartConfig[2])); 
            var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

            Console.WriteLine($"Task save check time proccessing...");

            // save check time
            RunTask(startTime, endTime);

            Console.WriteLine($"Task save check time finish.");
            Thread.Sleep(5000);
            Environment.Exit(0);
        }

        static void RunTask(TimeSpan startTime, TimeSpan endTime)
        {

            var currentTime = DateTime.Now.TimeOfDay;

            while (currentTime >= startTime && currentTime <= endTime)
            {
                try
                {
                    var studentsEntry = studentService.GetStudentCheckTimesEntry(GlobalConfig.Date);
                    var studentsExit = studentService.GetStudentCheckTimesExit(GlobalConfig.Date);
                    RunStudentsCheckTime(studentsEntry.Item1, studentsExit.Item1, SentMethod.New);
                }
                catch (Exception ex)
                {
                    // show exception on console
                    Console.WriteLine(ex.Message.ToString());
                    Console.ReadLine();
                }

                currentTime = DateTime.Now.TimeOfDay;
            }
        }

        private static void RunStudentsCheckTime(
            List<StudentCheckTime> studentCheckTimesEntry,
            List<StudentCheckTime> studentCheckTimesExit,
            SentMethod sentMethod)
        {
            if (studentCheckTimesEntry.Count > 0)
            {
                if (sentMethod == SentMethod.New)
                {
                    // save entry
                    studentService.SaveStudentStudentCheckTime(studentCheckTimesEntry);
                }
            }
            if (studentCheckTimesExit.Count > 0)
            {
                if (sentMethod == SentMethod.New)
                {
                    // save exit
                    studentService.SaveStudentStudentCheckTime(studentCheckTimesExit);
                }
            }
        }
    }
}

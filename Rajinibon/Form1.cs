using Rajinibon.Common;
using Rajinibon.DataAccess;
using Rajinibon.Models;
using Rajinibon.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rajinibon
{
    public partial class Form1 : Form
    {
        IStudentService _StudentService { get; set; }

        public Form1()
        {
            _StudentService = new StudentService();

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _StudentService.RemoveStudentsLess(GlobalConfig.Date.GetDate());

                var timeStartConfig = GlobalConfig.AppSettings("taskStartTime").Split(':');
                var timeEndConfig = GlobalConfig.AppSettings("taskEndTime").Split(':');

                var startTime = new TimeSpan(int.Parse(timeStartConfig[0]), int.Parse(timeStartConfig[1]), int.Parse(timeStartConfig[2]));
                var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

                SetUpTimer(startTime, endTime);
            }
            catch (Exception ex)
            {
                _StudentService.SaveExceptionLog(ex);
            }

        }

        private System.Threading.Timer timer;
        private void SetUpTimer(TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var _startTime = startTime;
                var _endTime = endTime;
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

                    while (_startTime + s.Elapsed <= _endTime)
                    {
                        RunStudentsCheckTime();
                        //await RunStudentsSentMessage();

                        Thread.Sleep(TimeSpan.FromSeconds(double.Parse(GlobalConfig.AppSettings("ThreadSleepTaskSec"))));
                    }

                    s.Stop();
                    //MessageBox.Show("Task Completed.");
                    Environment.Exit(0);
                }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _StudentService.SaveExceptionLog(ex);
            }
        }

        private void RunStudentsCheckTime()
        {
            try
            {
                var studentsEntry = _StudentService.GetStudentCheckTimesEntry(GlobalConfig.Date);
                var studentsExit = _StudentService.GetStudentCheckTimesExit(GlobalConfig.Date);
                //var diffEntry = _StudentService.GetStudentsEntryFromList(GlobalConfig.StudentCheckTimes).ToList().Count - _StudentService.GetStudentSentMessageEntryFromList(GlobalConfig.StudentSentMessages).ToList().Count;
                //var diffExit = _StudentService.GetStudentsExitFromList(GlobalConfig.StudentCheckTimes).ToList().Count - _StudentService.GetStudentSentMessageExitFromList(GlobalConfig.StudentSentMessages).ToList().Count;

                try
                {
                    if (studentsEntry.Item1.ToList().Count > 0)
                    {
                        // save students entry check time
                        _StudentService.SaveStudentStudentCheckTime(studentsEntry.Item1);
                    }

                    // students entry sent time
                    _StudentService.SentStudentNotifyMessage(studentsEntry.Item1, SentType.Entry);

                    if (studentsExit.Item1.ToList().Count > 0)
                    {
                        // save students exit check time
                        _StudentService.SaveStudentStudentCheckTime(studentsExit.Item1);
                    }

                    // students exit sent time
                    _StudentService.SentStudentNotifyMessage(studentsExit.Item1, SentType.Exit);

                }
                catch (Exception ex)
                {
                    _StudentService.SaveExceptionLog(ex);
                }
            }
            catch (Exception ex)
            {
                _StudentService.SaveExceptionLog(ex);
            }
        }

        private void RunStudentsSentMessage()
        {

            var studentsEntryDbf = _StudentService.GetStudentsEntryDbf(GlobalConfig.Date);
            var studentsExitDbf = _StudentService.GetStudentsExitDbf(GlobalConfig.Date);

            // students entry sent time
            if (studentsEntryDbf.ToList().Count > 0)
            {
                _StudentService.SentStudentNotifyMessage(studentsEntryDbf, SentType.Entry);
            }

            // students exit sent time
            if (studentsExitDbf.ToList().Count > 0)
            {
                _StudentService.SentStudentNotifyMessage(studentsExitDbf, SentType.Exit);
            }
        }
    }
}

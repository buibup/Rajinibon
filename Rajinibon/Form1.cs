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
        static readonly object _object = new object();

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
                        RunsAt();
                        Thread.Sleep(TimeSpan.FromSeconds(int.Parse(GlobalConfig.AppSettings("ThreadSleepTaskSec"))));
                    }

                    s.Stop();
                    //MessageBox.Show("Task Completed.");
                }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _StudentService.SaveExceptionLog(ex);
            }
        }

        private bool SentMessageSuccess()
        {


            return true;
        }

        private async void RunsAt()
        {
            try
            {
                var studentsEntry = await _StudentService.GetStudentCheckTimesEntry(GlobalConfig.Date);
                var studentsExit = await _StudentService.GetStudentCheckTimesExit(GlobalConfig.Date);

                try
                {
                    // students entry check time
                    if (studentsEntry.Item1.ToList().Count > 0)
                    {
                        await Task.Run(() =>
                        {
                            _StudentService.SaveStudentStudentCheckTime(studentsEntry.Item1);
                        });
                    }

                    // students entry sent time
                    if (studentsEntry.Item2.ToList().Count > 0)
                    {
                        SentMessage.StudentSentMessage(studentsEntry.Item2, SentType.Entry);

                        //_StudentService.SentStudentNotifyMessage(studentsEntry.Item2, SentType.Entry);
                    }

                    // students exit check time
                    if (studentsExit.Item1.ToList().Count > 0)
                    {
                        await Task.Run(() =>
                        {
                            _StudentService.SaveStudentStudentCheckTime(studentsExit.Item1);
                        });
                    }

                    // students exit sent time
                    if (studentsExit.Item2.ToList().Count > 0)
                    {
                        SentMessage.StudentSentMessage(studentsExit.Item2, SentType.Exit);
                        //_StudentService.SentStudentNotifyMessage(studentsExit.Item2, SentType.Exit);

                    }
                }
                catch (Exception ex)
                {
                    await _StudentService.SaveExceptionLog(ex);
                }
            }
            catch (Exception ex)
            {
                await Task.Run(() =>
                {
                    _StudentService.SaveExceptionLog(ex);
                });
            }
        }
    }
}

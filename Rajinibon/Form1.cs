using Rajinibon.Common;
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
                _StudentService.RemoveStudentsLess(GlobalConfig.Date);

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
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    }

                    s.Stop();
                    MessageBox.Show("Task Completed.");
                }, null, timeToGo, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _StudentService.SaveExceptionLog(ex);
            }
        }

        private async void RunsAt()
        {
            try
            {
                var studentsEntry = _StudentService.GetStudentCheckTimesEntry(GlobalConfig.Date).Result.ToList();

                var studentsExit = _StudentService.GetStudentCheckTimesExit(GlobalConfig.Date).Result.ToList();

                try
                {
                    // Thead safe => Monitor / Lock 
                    Monitor.Enter(_object);
                    if (studentsEntry.ToList().Count > 0)
                    {
                        await Task.Run(() =>
                        {
                            _StudentService.SaveStudentStudentCheckTime(studentsEntry);
                        });

                        await Task.Run(() =>
                        {
                            _StudentService.SentStudentNotifyMessage(studentsEntry, SentType.Entry);

                        });
                    }

                    if (studentsExit.ToList().Count > 0)
                    {
                        await Task.Run(() =>
                        {
                            _StudentService.SaveStudentStudentCheckTime(studentsExit);
                        });

                        await Task.Run(() =>
                        {
                            _StudentService.SentStudentNotifyMessage(studentsExit, SentType.Exit);
                        });
                    }
                    Monitor.Exit(_object);
                }
                catch(Exception ex)
                {
                    await _StudentService.SaveExceptionLog(ex);
                }
            }
            catch (Exception ex)
            {
                await Task.Run(() => {
                    _StudentService.SaveExceptionLog(ex);
                });
            }
            
        }
    }
}

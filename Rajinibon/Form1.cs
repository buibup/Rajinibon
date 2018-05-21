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
        IStudentService StudentService { get; set; }

        public Form1()
        {
            StudentService = new StudentService();

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var timeStartConfig = GlobalConfig.AppSettings("startTime").Split(':');
                var timeEndConfig = GlobalConfig.AppSettings("endTime").Split(':');

                var startTime = new TimeSpan(int.Parse(timeStartConfig[0]), int.Parse(timeStartConfig[1]), int.Parse(timeStartConfig[2]));
                var endTime = new TimeSpan(int.Parse(timeEndConfig[0]), int.Parse(timeEndConfig[1]), int.Parse(timeEndConfig[2]));

                SetUpTimer(startTime, endTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private System.Threading.Timer timer;
        private void SetUpTimer(TimeSpan startTime, TimeSpan endTime)
        {
            var _startTime = startTime;
            var _endTime = endTime;
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = _startTime - current.TimeOfDay;
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
                MessageBox.Show("Compleated");
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
            
        }

        private async void RunsAt()
        {
            var studentsEntry = await StudentService.GetStudentCheckTimesEntry(GlobalConfig.Date);

            var studentsExit = await StudentService.GetStudentCheckTimesExit(GlobalConfig.Date);

            if (studentsEntry.ToList().Count > 0)
            {
                await StudentService.SaveStudentStudentCheckTime(studentsEntry);
            }

            if (studentsExit.ToList().Count > 0)
            {
                await StudentService.SaveStudentStudentCheckTime(studentsExit);
            }
        }
    }
}

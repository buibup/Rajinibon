using Rajinibon.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        private async void Form1_Load(object sender, EventArgs e)
        {

            var studentsEntry = await StudentService.GetStudentCheckTimesEntry(GlobalConfig.Date);

            var studentsExit = await StudentService.GetStudentCheckTimesExit(GlobalConfig.Date);

            if(studentsEntry.ToList().Count > 0)
            {
                await StudentService.SaveStudentStudentCheckTime(studentsEntry);
            }
            
            if(studentsExit.ToList().Count > 0)
            {
                await StudentService.SaveStudentStudentCheckTime(studentsExit);
            }

        }
    }
}

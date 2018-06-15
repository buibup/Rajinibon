using Rajinibon.Common;
using Rajinibon.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rajinibon
{
    public static class GlobalConfig
    {
        public static string PreName { get { return AppSettings("PreName"); } }
        public static string DbfPath { get { return AppSettings("DbfPath"); } }
        public static string Date { get { return AppSettings("Date"); } }
        public static string CurrentDate { get { return Helper.GetDateNowStringUs("yyyyMMdd"); } }
        public static string DbConnection { get { return CnnString("DefaultConnection"); } }

        public static List<StudentCheckTime> StudentCheckTimes { get; set; } = new List<StudentCheckTime>();
        public static List<StudentSentMessage> StudentSentMessages { get; set; } = new List<StudentSentMessage>();

        public static int StudentCount { get; set; } = 1;

        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static string AppSettings(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rajinibon.Common
{
    public static class Helper
    {
        public static string GetFullPath(this string rootPath, string date)
        {
            if (date.ToLower() == "current")
            {
                return $"{rootPath}{GlobalConfig.PreName}{DateTime.Now.ToString("yyyyMMdd")}.dbf";
            }
            else
            {
                return $"{rootPath}{GlobalConfig.PreName}{date}.dbf";
            }
        }

        public static string GetDate(this string date)
        {
            if (date.ToLower() == "current")
            {
                return $"{DateTime.Now.ToString("yyyyMMdd")}";
            }
            else
            {
                return $"{date}";
            }
        }

        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static bool Between(this TimeSpan time, TimeSpan timeStart, TimeSpan timeEnd, bool inclusive = false)
        {
            return inclusive
                ? timeStart <= time && time <= timeEnd
                : timeStart < time && time < timeEnd;
        }

        public static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }

        public static string ToApplicationPath(this string fileName)
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                                .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return Path.Combine(appRoot, fileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NewsAggregator.Util
{
    public class Logger
    {
        public static void Log(string text)
        {
            write(DateTime.Now.ToString("MM dd HH:mm:ss") + " " + text);
        }

        public static void Error(string text)
        {
            write("Error!: " + DateTime.Now.ToString("MM dd HH:mm:ss") + " " + text);
        }

        private static void write(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", text + Environment.NewLine);
        }
    }
}
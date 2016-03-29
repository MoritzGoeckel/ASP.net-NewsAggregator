using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.Util
{
    public class Logger
    {
        public static void Log(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
        }

        public static void Error(string text)
        {
            System.Diagnostics.Debug.WriteLine(text);
        }
    }
}
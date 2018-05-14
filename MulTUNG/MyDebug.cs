using System;
using System.Diagnostics;

namespace MulTUNG
{
    public static class MyDebug
    {
        private static string Time => DateTime.Now.ToString(@"hh\:mm\:ss");

        private static string Side => Network.IsClient ? "CLIENT" : "SERVER";

        public static void Log(object line)
        {
            Debug.WriteLine($"[{Side}] [{Time}] {line}");
        }
    }
}

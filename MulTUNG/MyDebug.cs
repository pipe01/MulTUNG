﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MulTUNG
{
    public static class MyDebug
    {
        private static string Time => DateTime.Now.ToString(@"hh\:mm\:ss");

        private static string Side => Network.IsClient ? "CLIENT" : "SERVER";

        public static void Log(string line)
        {
            Debug.WriteLine($"[{Side}] [{Time}] {line}");
        }
    }
}

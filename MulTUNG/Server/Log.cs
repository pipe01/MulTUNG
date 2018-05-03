using PiTung.Console;
using System;
using System.Collections.Generic;

namespace Server
{
    public static class Log
    {
        public static void WriteLine(string line)
        {
            IGConsole.Log("[SERVER] " + line);
        }
    }
}

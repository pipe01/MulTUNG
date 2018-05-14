using MulTUNG;
using PiTung.Console;

namespace Server
{
    public static class Log
    {
        private static string Side => Network.IsClient ? "CLIENT" : "SERVER";

        public static void WriteLine(object msg)
        {
            IGConsole.Log($"[{Side}] {msg}");
        }
    }
}

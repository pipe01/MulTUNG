using System;
using System.Collections.Generic;

namespace Server
{
    public static class Log
    {
        public static Queue<string> LineQueue = new Queue<string>();

        public static bool WriteToConsole { get; set; } = true;

        public static void FlushQueue()
        {
            while (LineQueue.Count > 0)
            {
                Console.WriteLine(LineQueue.Dequeue());
            }
        }

        public static void WriteLine(string line)
        {
            if (WriteToConsole)
            {
                Console.WriteLine(line);
            }
            else
            {
                LineQueue.Enqueue(line);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public static class Log
    {
        private static Queue<string> Lines = new Queue<string>();

        public static bool PrintLines { get; set; } = true;

        public static void FlushQueue()
        {
            while (Lines.Count > 0)
            {
                Console.WriteLine(Lines.Dequeue());
            }
        }

        public static void WriteLine(string line)
        {
            if (PrintLines)
                Console.WriteLine(Lines);
            else
                Lines.Enqueue(line);
        }
    }
}

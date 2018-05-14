using System;
using System.Linq;

namespace MulTUNG.Headless
{
    public static class Headlesser
    {
        public static bool IsHeadless { get; } = Environment.GetCommandLineArgs().Contains("-server");
    }
}

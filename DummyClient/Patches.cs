using Harmony;
using MulTUNG;
using PiTung.Console;
using System;
using UnityEngine;

namespace DummyClient
{
    [HarmonyPatch(typeof(IGConsole), "Log", new[] { typeof(object) })]
    static class IGConsoleLogPatch
    {
        static bool Prefix(object msg)
        {
            Console.WriteLine("[IG] " + msg);

            return false;
        }
    }

    [HarmonyPatch(typeof(Time))]
    [HarmonyPatch("time", PropertyMethod.Getter)]
    static class GetTimePatch
    {
        static bool Prefix(ref float __result)
        {
            __result = (float)Program.TimeStopwatch.Elapsed.TotalSeconds;

            return false;
        }
    }

    [HarmonyPatch(typeof(Console), "WriteLine", new[] { typeof(string) })]
    static class WriteLinePatch
    {
        static bool Prefix(string value)
        {
            if (!Log.PrintLines)
                Log.WriteLine(value);

            return Log.PrintLines;
        }
    }

    [HarmonyPatch(typeof(PlayerManager), "NewPlayer")]
    static class NewPlayerPatch
    {
        static bool Prefix() => false;
    }

    [HarmonyPatch(typeof(PlayerManager), "UpdatePlayer")]
    static class UpdatePlayerPatch
    {
        static bool Prefix() => false;
    }

    //[HarmonyPatch(typeof(PlayerManager), "WaveGoodbye")]
    //static class WaveGoodbyePatch
    //{
    //    static bool Prefix() => false;
    //}
}

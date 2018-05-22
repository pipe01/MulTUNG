using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StateKey = System.Collections.Generic.KeyValuePair<int, byte>;

namespace MulTUNG.Patches
{
    [Target(typeof(CircuitOutput))]
    internal static class CircuitOutputPatch
    {
        private static Dictionary<CircuitOutput, StateKey> KeysCache = new Dictionary<CircuitOutput, KeyValuePair<int, byte>>();

        [PatchMethod]
        public static void set_On(CircuitOutput __instance, bool value)
        {
            if (KeysCache.TryGetValue(__instance, out var key))
            {

            }

            ComponentPlacer.FullComponent(__instance.transform);
        }
    }
}

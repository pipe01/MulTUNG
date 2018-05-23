using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Patches
{
    [Target(typeof(CircuitOutput))]
    internal static class CircuitOutputPatch
    {
        [PatchMethod]
        public static bool set_On(CircuitOutput __instance, bool value)
        {
            if (Network.IsClient && ComponentActions.CurrentlyUpdating != __instance)
            {
                return false;
            }

            if (Network.IsServer)
            {
                CircuitStatePacket.SetOutputState(__instance, value);
            }

            return true;
        }
    }
}

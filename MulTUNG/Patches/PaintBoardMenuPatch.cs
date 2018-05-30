using MulTUNG.Packets;
using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Patches
{
    [Target(typeof(PaintBoardMenu))]
    internal static class PaintBoardMenuPatch
    {
        [PatchMethod]
        public static void ColorBoard(PaintBoardMenu __instance)
        {
            if (Physics.Raycast(FirstPersonInteraction.Ray(), out var raycastHit, Settings.ReachDistance, Wire.IgnoreWiresLayermask) && raycastHit.collider.tag == "CircuitBoard")
            {
                var netObj = raycastHit.collider.GetComponent<NetObject>();
                IGConsole.Log("Hit " + netObj);

                if (netObj == null)
                    return;
                
                var color = __instance.Colors[__instance.SelectedThing];

                Network.SendPacket(new PaintBoardPacket
                {
                    BoardID = netObj.NetID,
                    Color = color
                });
            }
        }
    }
}

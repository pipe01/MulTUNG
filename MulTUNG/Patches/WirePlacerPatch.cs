using MulTUNG.Packets;
using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(WirePlacer))]
    internal static class WirePlacerPatch
    {
        [PatchMethod]
        public static void ConnectionFinal()
        {
            if (WirePlacer.CurrentWirePlacementIsValid())
            {
                var wireBeingPlaced = ModUtilities.GetStaticFieldValue<GameObject>(typeof(WirePlacer), "WireBeingPlaced");
                var wire = wireBeingPlaced.GetComponent<Wire>();

                Network.SendPacket(PlaceWirePacket.BuildFromLocalWire(wire));
            }
        }
    }
}

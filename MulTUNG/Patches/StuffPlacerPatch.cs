using MulTUNG.Packeting.Packets;
using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(StuffPlacer))]
    internal static class StuffPlacerPatch
    {
        [PatchMethod]
        public static void PlaceThingBeingPlaced(ref GameObject __state)
        {
            __state = StuffPlacer.GetThingBeingPlaced;
        }

        [PatchMethod("PlaceThingBeingPlaced", PatchType.Postfix)]
        public static void PlaceThingBeingPlacedPostfix(ref GameObject __state)
        {
            var objInfo = __state.GetComponent<ObjectInfo>();

            if (objInfo != null && objInfo.ComponentType != ComponentType.CircuitBoard)
            {
                Network.SendPacket(PlaceComponentPacket.BuildFromLocalComponent(__state));
            }
        }
    }
}

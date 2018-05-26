using Harmony;
using MulTUNG.Packets;
using UnityEngine;

namespace MulTUNG.Patches
{

    [HarmonyPatch(typeof(StuffDeleter), "DestroyWire", new[] { typeof(GameObject) })]
    internal static class StuffDeleterDestroyWirePatch
    {
        static void Prefix(GameObject wire)
        {
            if (!PatchesCommon.IsGameplayDeleting)
                return;

            var netObj = wire.GetComponent<NetObject>();

            if (netObj != null)
            {
                Network.SendPacket(new DeleteWirePacket
                {
                    WireNetID = netObj.NetID
                });
            }
        }
    }
}

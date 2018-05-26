using MulTUNG.Packets;
using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(StuffDeleter))]
    internal static class StuffDeleterPatch
    {
        [PatchMethod]
        public static void RunGameplayDeleting()
        {
            PatchesCommon.IsGameplayDeleting = true;
        }

        [PatchMethod("RunGameplayDeleting", PatchType.Postfix)]
        public static void RunGameplayDeletingPostfix()
        {
            PatchesCommon.IsGameplayDeleting = false;
        }

        [PatchMethod]
        public static void DeleteThing(GameObject DestroyThis, ref bool __state)
        {
            __state = DestroyThis?.tag == "CircuitBoard";
        }

        [PatchMethod("DeleteThing", PatchType.Postfix)]
        public static void DeleteThingPostfix(GameObject DestroyThis)
        {
            if (DestroyThis == null)
                return;
            
            var netObj = DestroyThis.GetComponent<NetObject>();

            if (netObj == null)
                return;
            
            if (DestroyThis.tag == "CircuitBoard")
            {
                if (DestroyThis.transform.childCount > 0 && ((DestroyThis.transform.childCount != 1 || !DestroyThis.transform.GetChild(0).gameObject == StuffPlacer.GetThingBeingPlaced)))
                {
                    return;
                }

                if (!(NetUtilitiesComponent.Instance.CurrentJob is DeleteBoardJob))
                {
                    Network.SendPacket(new DeleteBoardPacket
                    {
                        BoardID = netObj.NetID
                    });
                }
            }
            else
            {
                Network.SendPacket(new DeleteComponentPacket
                {
                    ComponentNetID = netObj.NetID
                });
            }
        }
    }
}

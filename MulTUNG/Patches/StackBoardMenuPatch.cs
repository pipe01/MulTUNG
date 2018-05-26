using MulTUNG.Packets;
using PiTung;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MulTUNG.Patches
{

    [Target(typeof(StackBoardMenu))]
    internal static class StackBoardMenuPatch
    {
        [PatchMethod]
        public static void Place(StackBoardMenu __instance)
        {
            if (!ModUtilities.GetFieldValue<bool>(__instance, "CurrentPlacementIsValid"))
                return;

            var allBoards = ModUtilities.GetFieldValue<List<GameObject>>(__instance, "AllSubBoardsInvolvedWithStacking");
            var parentBoard = ModUtilities.GetFieldValue<GameObject>(__instance, "BoardBeingStacked");
            var firstBoard = allBoards.First(o => o != parentBoard);

            foreach (var item in firstBoard.GetComponentsInChildren<ObjectInfo>())
            {
                var netObj = item.GetComponent<NetObject>() ?? item.gameObject.AddComponent<NetObject>();
                netObj.NetID = NetObject.GetNewID();
            }

            var packet = PlaceBoardPacket.BuildFromBoard(firstBoard.GetComponent<CircuitBoard>(), parentBoard.transform);
            Network.SendPacket(packet);
        }
    }
}

using MulTUNG.Packets;
using PiTung;
using UnityEngine;

namespace MulTUNG.Patches
{
    [Target(typeof(BoardPlacer))]
    internal static class BoardPlacerPatch
    {
        [PatchMethod]
        public static void PlaceBoard()
        {
            if (StuffPlacer.OkayToPlace)
            {
                foreach (var item in BoardPlacer.BoardBeingPlaced.GetComponentsInChildren<ObjectInfo>())
                {
                    var netObj = item.GetComponent<NetObject>() ?? item.gameObject.AddComponent<NetObject>();
                    netObj.NetID = NetObject.GetNewID();
                }

                foreach (var item in BoardPlacer.BoardBeingPlaced.GetComponentsInChildren<NetObject>())
                {
                    item.enabled = true;
                }

                var boardComp = BoardPlacer.BoardBeingPlaced.GetComponent<CircuitBoard>();
                var parent = BoardPlacer.ReferenceObject.transform.parent;

                var packet = PlaceBoardPacket.BuildFromBoard(boardComp, parent);

                Network.SendPacket(packet);

                MulTUNG.DumpNetobjs();
                foreach (var item in boardComp.GetComponentsInChildren<CircuitOutput>())
                {
                    CircuitStatePacket.SetOutputState(item, item.On, true);
                }
            }
        }

        [PatchMethod]
        public static void NewBoardBeingPlaced(GameObject NewBoard)
        {
            if (PatchesCommon.IsTryingToMoveBoard)
            {
                var net = NewBoard.GetComponent<NetObject>();

                if (net != null)
                {
                    foreach (var item in NewBoard.GetComponentsInChildren<NetObject>())
                    {
                        item.enabled = false;
                    }

                    Network.SendPacket(new DeleteBoardPacket { BoardID = net.NetID });
                }
            }
            else if (PatchesCommon.IsCloning)
            {
                PatchesCommon.IsCloning = false;

                foreach (var item in NewBoard.GetComponentsInChildren<NetObject>())
                {
                    GameObject.Destroy(item);
                }
            }
        }
    }
}

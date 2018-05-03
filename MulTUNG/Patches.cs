using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using SavedObjects;
using UnityEngine;

namespace MulTUNG
{
    [Target(typeof(BoardPlacer))]
    internal static class BoardPlacerPatch
    {
        public static bool IsTryingToMoveBoard = false;

        [PatchMethod]
        public static void PlaceBoard()
        {
            if (StuffPlacer.OkayToPlace)
            {
                var boardComp = BoardPlacer.BoardBeingPlaced.GetComponent<CircuitBoard>();
                int id = Random.Range(int.MinValue, int.MaxValue);

                if (boardComp.GetComponent<NetObject>() == null)
                    boardComp.gameObject.AddComponent<NetObject>().NetID = id;

                var parent = BoardPlacer.ReferenceObject.transform.parent;

                var packet = new PlaceBoardPacket
                {
                    AuthorID = MulTUNG.NetClient.PlayerID,
                    BoardID = id,
                    ParentBoardID = parent?.GetComponent<NetObject>()?.NetID ?? 0,
                    Width = boardComp.x,
                    Height = boardComp.z,
                    Position = boardComp.transform.position,
                    EulerAngles = boardComp.transform.eulerAngles
                };
                
                MulTUNG.NetClient.SendPacket(packet);
            }
        }

        [PatchMethod]
        public static void NewBoardBeingPlaced(GameObject NewBoard)
        {
            if (IsTryingToMoveBoard)
            {
                var net = NewBoard.GetComponent<NetObject>();

                if (net != null)
                {
                    IGConsole.Log("Send delete board with id " + net.NetID);
                    MulTUNG.NetClient.SendPacket(new DeleteBoardPacket { BoardID = net.NetID });
                }
            }
        }
    }

    [Target(typeof(BoardMenu))]
    internal static class BoardMenuPatch
    {
        [PatchMethod]
        public static void ExecuteSelectedAction()
        {
            if (BoardMenu.Instance.SelectedThing == 2)
            {
                BoardPlacerPatch.IsTryingToMoveBoard = true;
            }
        }

        [PatchMethod(OriginalMethod = "ExecuteSelectedAction", PatchType = PatchType.Postfix)]
        public static void ExecuteSelectedActionPostfix()
        {
            BoardPlacerPatch.IsTryingToMoveBoard = false;
        }
    }

    [Target(typeof(StuffPlacer))]
    internal static class StuffPlacerPatch
    {
        [PatchMethod]
        public static void PlaceThingBeingPlaced(ref GameObject __state)
        {
            __state = StuffPlacer.GetThingBeingPlaced;
        }

        [PatchMethod(OriginalMethod = "PlaceThingBeingPlaced", PatchType = PatchType.Postfix)]
        public static void PlaceThingBeingPlacedPostfix(ref GameObject __state)
        {
            var objInfo = __state.GetComponent<ObjectInfo>();

            if (objInfo != null && objInfo.ComponentType != ComponentType.CircuitBoard)
            {
                var netObj = __state.AddComponent<NetObject>();
                netObj.NetID = Random.Range(int.MinValue, int.MaxValue);

                MulTUNG.NetClient.SendPacket(new PlaceComponentPacket
                {
                    NetID = netObj.NetID,
                    SavedObject = SavedObjectUtilities.CreateSavedObjectFrom(objInfo),
                    LocalPosition = __state.transform.localPosition,
                    EulerAngles = __state.transform.localEulerAngles,
                    ParentBoardID = __state.transform.parent?.gameObject.GetComponent<NetObject>()?.NetID ?? 0
                });
            }
        }
    }

    [Target(typeof(StuffDeleter))]
    internal static class StuffDeleterPatch
    {
        [PatchMethod]
        public static void DeleteThing(GameObject DestroyThis, ref bool __state)
        {
            __state = DestroyThis?.tag == "CircuitBoard";
        }

        [PatchMethod(OriginalMethod = "DeleteThing", PatchType = PatchType.Postfix)]
        public static void DeleteThingPostfix(GameObject DestroyThis)
        {
            if (DestroyThis == null)
                return;
            
            var netObj = DestroyThis.GetComponent<NetObject>();

            if (netObj == null)
                return;
            
            if (DestroyThis.tag == "CircuitBoard")
            {
                if (!(NetUtilitiesComponent.Instance.CurrentJob is DeleteBoardJob))
                {
                    MulTUNG.NetClient.SendPacket(new DeleteBoardPacket
                    {
                        BoardID = netObj.NetID
                    });
                }
            }
            else //if (DestroyThis.GetComponent<ObjectInfo>()?.ComponentType != ComponentType.CircuitBoard)
            {
                MulTUNG.NetClient.SendPacket(new DeleteComponentPacket
                {
                    ComponentNetID = netObj.NetID
                });
            }
        }
    }

    [Target(typeof(BehaviorManager))]
    internal static class BehaviorManagerPatch
    {
        [PatchMethod]
        public static void Awake()
        {
            MyFixedUpdate.NextInstanceIsNice = true;
        }
    }
}

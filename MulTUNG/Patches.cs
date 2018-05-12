using Harmony;
using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
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
                    AuthorID = NetworkClient.Instance.PlayerID,
                    BoardID = id,
                    ParentBoardID = parent?.GetComponent<NetObject>()?.NetID ?? 0,
                    Width = boardComp.x,
                    Height = boardComp.z,
                    Position = boardComp.transform.position,
                    EulerAngles = boardComp.transform.eulerAngles
                };
                
                Network.SendPacket(packet);
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
                    Network.SendPacket(new DeleteBoardPacket { BoardID = net.NetID });
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

        [PatchMethod("ExecuteSelectedAction", PatchType.Postfix)]
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

    [Target(typeof(StuffDeleter))]
    internal static class StuffDeleterPatch
    {
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

    [HarmonyPatch(typeof(StuffDeleter), "DestroyWire", new[] { typeof(GameObject) })]
    internal static class StuffDeleterDestroyWirePatch
    {
        static void Prefix(GameObject wire)
        {
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

    [Target(typeof(BehaviorManager))]
    internal static class BehaviorManagerPatch
    {
        [PatchMethod]
        public static bool OnCircuitLogicUpdate()
        {
            return true;//!Network.IsClient;
        }
    }

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

    [Target(typeof(StuffRotater))]
    internal static class StuffRotaterPatch
    {
        [PatchMethod]
        public static void RotateThing(GameObject RotateThis, ref Vector3 __state)
        {
            __state = RotateThis.transform.localEulerAngles;
        }

        [PatchMethod("RotateThing", PatchType.Postfix)]
        public static void RotateThingPostfix(GameObject RotateThis, ref Vector3 __state)
        {
            if (RotateThis.transform.localEulerAngles != __state && RotateThis.tag != "Wire")
            {
                var netObj = RotateThis.GetComponent<NetObject>();

                if (netObj != null)
                {
                    Network.SendPacket(new RotateComponentPacket
                    {
                        ComponentID = netObj.NetID,
                        EulerAngles = RotateThis.transform.localEulerAngles
                    });
                }
            }
        }
    }

    [Target(typeof(SaveManager))]
    internal static class SaveManagerPatch
    {
        [PatchMethod]
        static bool SaveAllSynchronously() => !Network.IsClient;
    }

    [Target(typeof(Button))]
    internal static class ButtonPatch
    {
        [PatchMethod(PatchType.Postfix)]
        public static void ButtonDown(Button __instance)
        {
            var netObj = __instance.GetComponent<NetObject>();

            if (netObj == null || ComponentActions.CurrentlyActing.Contains(netObj.NetID))
                return;
            
            var packet = new UserInputPacket
            {
                NetID = netObj.NetID,
                Receiver = UserInputPacket.UserInputReceiver.Button,
                State = __instance.output.On
            };

            Network.SendPacket(packet);
        }
    }
}

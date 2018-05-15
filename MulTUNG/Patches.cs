using Harmony;
using MulTUNG.Packeting.Packets;
using MulTUNG.Utils;
using PiTung;
using PiTung.Console;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
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
                IGConsole.Log(BoardPlacer.BoardBeingPlaced.GetComponent<NetObject>());

                var boardComp = BoardPlacer.BoardBeingPlaced.GetComponent<CircuitBoard>();
                var parent = BoardPlacer.ReferenceObject.transform.parent;

                var packet = PlaceBoardPacket.BuildFromBoard(boardComp, parent);

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
            if (ComponentActions.HasCalledCircuitUpdate)
            {
                ComponentActions.HasCalledCircuitUpdate = false;
                return true;
            }

            return !Network.IsClient;
        }

        [PatchMethod("OnCircuitLogicUpdate", PatchType.Postfix)]
        public static void OnCircuitLogicUpdatePostfix()
        {
            if (Network.IsServer)
            {
                Network.SendPacket(CircuitStatePacket.Build());
            }
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
        [PatchMethod]
        public static bool Update(Button __instance)
        {
            if (ComponentActions.PushedDownButtons.Contains(__instance))
                return false;

            return true;
        }

        [PatchMethod(PatchType.Postfix)]
        public static void ButtonDown(Button __instance) => DoButton(__instance);

        [PatchMethod(PatchType.Postfix)]
        public static void ButtonUp(Button __instance) => DoButton(__instance);

        private static void DoButton(Button __instance)
        {
            var netObj = __instance.transform.parent.GetComponent<NetObject>();

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

    [Target(typeof(Switch))]
    internal static class SwitchPatch
    {
        [PatchMethod]
        public static void UpdateLever(Switch __instance)
        {
            var netObj = __instance.transform.parent.GetComponent<NetObject>();

            if (netObj == null || ComponentActions.CurrentlyActing.Contains(netObj.NetID))
                return;

            var packet = new UserInputPacket
            {
                NetID = netObj.NetID,
                Receiver = UserInputPacket.UserInputReceiver.Switch,
                State = __instance.On
            };

            Network.SendPacket(packet);
        }
    }

    [Target(typeof(NoisemakerMenu))]
    internal static class NoisemakerMenuPatch
    {
        [PatchMethod(PatchType.Postfix)]
        public static void Done()
        {
            var noisemaker = NoisemakerMenu.NoisemakerBeingEdited;
            var netObj = noisemaker.transform.parent.GetComponent<NetObject>();
            
            if (netObj == null)
                return;

            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Noisemaker,
                Data = new List<object>
                {
                    noisemaker.ToneFrequency
                }
            });
        }
    }

    [Target(typeof(EditDisplayColorMenu))]
    internal static class EditDisplayColorMenuPatch
    {
        [PatchMethod(PatchType.Postfix)]
        public static void DoneMenu(EditDisplayColorMenu __instance)
        {
            var display = __instance.DisplayBeingEdited;
            var netObj = display.transform.parent.GetComponent<NetObject>();

            if (netObj == null)
                return;

            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Display,
                Data = new List<object>
                {
                    display.DisplayColor
                }
            });
        }
    }

    [Target(typeof(TextEditMenu))]
    internal static class TextEditMenuPatch
    {
        public static Label LabelBeingEdited;

        [PatchMethod]
        public static void Done(EditDisplayColorMenu __instance)
        {
            var netObj = LabelBeingEdited.GetComponent<NetObject>();

            if (netObj == null)
                return;
            
            Network.SendPacket(new ComponentDataPacket
            {
                NetID = netObj.NetID,
                ComponentType = ComponentType.Label,
                Data = new List<object>
                {
                    LabelBeingEdited.text.text,
                    LabelBeingEdited.text.fontSize
                }
            });
        }
    }

    [Target(typeof(Label))]
    internal static class LabelPatch
    {
        [PatchMethod]
        public static void Interact(Label __instance)
        {
            TextEditMenuPatch.LabelBeingEdited = __instance;
        }
    }

    [Target(typeof(PauseMenu))]
    internal static class PauseMenuPatch
    {
        [PatchMethod]
        public static bool QuitToMainMenu()
        {
            if (Network.IsClient)
            {
                NetworkClient.Instance.Disconnect();
                Time.timeScale = 1;

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SavedObjectUtilities), "CreateSavedObjectFrom", new[] { typeof(ObjectInfo) })]
    internal static class SavedObjectUtilitiesCreateSavedObjectFromPatch
    {
        static void Postfix(ObjectInfo worldsave, ref SavedObjectV2 __result)
        {
            var netObj = worldsave.GetComponent<NetObject>();

            if (netObj != null)
            {
                if (__result.Children == null)
                    __result.Children = new SavedObjectV2[0];

                SavedObjectV2[] newChildren = new SavedObjectV2[__result.Children.Length + 1];
                Array.Copy(__result.Children, 0, newChildren, 1, __result.Children.Length);

                IGConsole.Log(netObj.NetID);
                newChildren[0] = new SavedNetObject(netObj.NetID);

                __result.Children = newChildren;
            }
        }
    }

    [Target(typeof(SavedObjectUtilities))]
    internal static class SavedObjectUtilitiesPatch
    {
        [PatchMethod]
        public static bool LoadSavedObject(SavedObjectV2 save)
        {
            return !(save is SavedNetObject);
        }

        [PatchMethod("LoadSavedObject", PatchType.Postfix)]
        public static void LoadSavedObject(SavedObjectV2 save, ref GameObject __result)
        {
            foreach (var item in save.Children ?? new SavedObjectV2[0])
            {
                MyDebug.Log(item.GetType().Name);
            }

            if (save.Children?.FirstOrDefault() is SavedNetObject net)
            {
                IGConsole.Log("Net object! " + net.NetID);
                __result.AddComponent<NetObject>().NetID = net.NetID;
            }
        }
    }
}

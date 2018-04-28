using Common.Packets;
using PiTung;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Client
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
                int id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

                if (boardComp.GetComponent<NetBoard>() == null)
                    boardComp.gameObject.AddComponent<NetBoard>().BoardID = id;

                var packet = new PlaceBoardPacket
                {
                    AuthorID = Client.NetClient.PlayerID,
                    BoardID = id,
                    Width = boardComp.x,
                    Height = boardComp.z,
                    Position = boardComp.transform.position,
                    EulerAngles = boardComp.transform.eulerAngles
                };

                Client.NetClient.SendPacket(packet);
            }
        }

        [PatchMethod]
        public static void NewBoardBeingPlaced(GameObject NewBoard)
        {
            if (IsTryingToMoveBoard)
            {
                var net = NewBoard.GetComponent<NetBoard>();

                if (net != null)
                {
                    IGConsole.Log("Send delete board with id " + net.BoardID);
                    Client.NetClient.SendPacket(new DeleteBoardPacket { BoardID = net.BoardID });
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
}

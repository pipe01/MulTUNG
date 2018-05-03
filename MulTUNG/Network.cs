using MulTUNG;
using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MulTUNG
{
    public static class Network
    {
        public static bool Connected => IsClient || IsServer;
        public static bool IsClient => NetworkClient.Instance?.Connected ?? false;
        public static bool IsServer => NetworkServer.Instance?.Running ?? false;

        public const int ServerPlayerID = 1;

        public static void ProcessPacket(Packet packet, int playerId)
        {
            if (packet is PlayerWelcomePacket wel)
            {
                IGConsole.Log("Your player ID is " + wel.YourID);

                MulTUNG.NetClient.SetID(wel.YourID);
            }
            if (packet.SenderID == playerId)
            {
                return;
            }

            switch (packet)
            {
                case PlayerStatePacket state:
                    if (state.PlayerID != playerId)
                    {
                        PlayerManager.UpdatePlayer(state);
                    }

                    break;
                case PlaceBoardPacket board:
                    if (board.AuthorID != playerId)
                    {
                        NetUtilitiesComponent.Instance.Enqueue(new PlaceBoardJob(board));
                    }

                    break;
                case DeleteBoardPacket del:
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteBoardJob(del.BoardID));

                    break;
                case PlaceComponentPacket placeComp:
                    NetUtilitiesComponent.Instance.Enqueue(new PlaceComponentJob(placeComp));

                    break;
                case DeleteComponentPacket delComp:
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteComponentJob(delComp.ComponentNetID));

                    break;
                case CircuitUpdatePacket imnotgonnausethis:
                    MyFixedUpdate.Instance?.ForceUpdate();

                    break;
            }
        }

        public static void StartPositionUpdateThread(int updateInterval)
        {
            new Thread(() =>
            {
                while (Connected)
                {
                    Thread.Sleep(updateInterval);

                    if (ModUtilities.IsOnMainMenu)
                        continue;

                    var packet = new PlayerStatePacket
                    {
                        PlayerID = NetworkClient.Instance?.PlayerID ?? ServerPlayerID,
                        Time = Time.time,
                        Position = FirstPersonInteraction.FirstPersonCamera.transform.position,
                        EulerAngles = FirstPersonInteraction.FirstPersonCamera.transform.eulerAngles
                    };

                    SendPacket(packet);
                }
            }).Start();
        }

        public static void SendPacket(Packet packet)
        {
            if (IsClient)
                NetworkClient.Instance.SendPacket(packet);
            else if (IsServer)
                NetworkServer.Instance.Broadcast(packet, ServerPlayerID);
        }
    }
}

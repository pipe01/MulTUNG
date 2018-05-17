﻿using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using MulTUNG.Utils;
using PiTung;
using Server;
using System.Threading;
using UnityEngine;

namespace MulTUNG
{
    public static class Network
    {
        public static bool Connected => IsClient || IsServer;
        public static bool IsClient => NetworkClient.Instance?.Connected ?? false;
        public static bool IsServer => NetworkServer.Instance?.Running ?? false;

        public static bool IsPaused { get; private set; }

        public static int PlayerID => IsClient ? NetworkClient.Instance.PlayerID : ServerPlayerID;
        
        public const int ServerPlayerID = 0;
        
        public static void ProcessPacket(Packet packet, int playerId)
        {
            if (packet == null)
                return;

            if (packet is PlayerWelcomePacket wel)
            {
                NetworkClient.Instance.SetID(wel.YourID);
                return;
            }
            
            if (packet.SenderID == playerId && !(packet is StateListPacket))
                return;
            
            switch (packet)
            {
                case PlayerStatePacket state:
                    if (IsServer)
                    {
                        NetworkServer.Instance.UpdatePlayerState(state);
                    }

                    break;
                case StateListPacket states:
                    PlayerManager.UpdateStates(states);

                    break;
                case SignalPacket signal:
                    HandleSignalPacket(signal);

                    break;
                case PlayerDisconnectPacket disconnect:
                    PlayerManager.WaveGoodbye(disconnect.PlayerID);

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
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteComponentJob(delComp));

                    break;
                case PlaceWirePacket placeWire:
                    NetUtilitiesComponent.Instance.Enqueue(new PlaceWireJob(placeWire));
                    
                    break;
                case DeleteWirePacket deleteWire:
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteWireJob(deleteWire));

                    break;
                case RotateComponentPacket rotateComp:
                    NetUtilitiesComponent.Instance.Enqueue(new RotateComponentJob(rotateComp));

                    break;
                case WorldDataPacket world:
                    World.Deserialize(world.Data);

                    break;
                case UserInputPacket input:
                    MulTUNG.SynchronizationContext.Post(o => ComponentActions.DoAction(o as UserInputPacket), input);

                    break;
                case ComponentDataPacket compdata:
                    MulTUNG.SynchronizationContext.Post(o => ComponentActions.DoData(o as ComponentDataPacket), compdata);
                    break;
                case CircuitStatePacket circuit:
                    MulTUNG.SynchronizationContext.Post(o => ComponentActions.UpdateStates(o as CircuitStatePacket), circuit);

                    break;
                case PlayerDataPacket player:
                    if (IsServer)
                        NetworkServer.Instance.ReceivedPlayerData(player);

                    break;
            }
        }

        public static void HandleSignalPacket(SignalPacket signal)
        {
            switch (signal.Data)
            {
                case SignalData.CircuitUpdate:
                    break;
                case SignalData.RequestWorld:
                    if (IsServer)
                        NetworkServer.Instance.SendWorld(signal.SenderID);

                    break;
                case SignalData.Pause:
                    IsPaused = true;
                    Time.timeScale = 0;

                    break;
                case SignalData.Resume:
                    IsPaused = false;
                    Time.timeScale = 1;

                    break;
            }
        }
        
        public static void StartPositionUpdateThread(int updateInterval)
        {
            //The timer will start after 500ms have elapsed
            Timer timer = null;
            timer = new Timer(_ =>
            {
                if (!Connected || ModUtilities.IsOnMainMenu)
                {
                    timer.Dispose();
                    return;
                }
                
                SendState();

                if (IsServer)
                {
                    NetworkServer.Instance.SendStatesToPlayers();
                }
            }, null, 500, updateInterval);
        }

        private static void SendState()
        {
            var packet = new PlayerStatePacket
            {
                PlayerID = PlayerID,
                Position = FirstPersonInteraction.FirstPersonCamera.transform.position,
                EulerAngles = FirstPersonInteraction.FirstPersonCamera.transform.eulerAngles
            };

            if (IsClient)
                Network.SendPacket(packet);
            else
                Network.ProcessPacket(packet, 0);
        }

        public static void PauseGame()
        {
            if (IsServer)
                Network.SendPacket(new SignalPacket(SignalData.Pause));
        }

        public static void ResumeGame()
        {
            if (IsServer)
                Network.SendPacket(new SignalPacket(SignalData.Resume));
        }

        public static void SendPacket(Packet packet)
        {
            packet.Time = Time.time;

            if (IsClient)
            {
                NetworkClient.Instance.Send(packet);
            }
            else if (IsServer)
            {
                NetworkServer.Instance.Send(packet);
            }
        }
    }
}

using MulTUNG;
using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using MulTUNG.Utils;
using PiTung;
using PiTung.Console;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace MulTUNG
{
    public static class Network
    {
        private class PacketWaiter
        {
            public Predicate<Packet> Predicate;
            public bool ContinueProcessing;
            public ManualResetEvent Mre;

            public Packet Packet;
        }

        public static bool Connected => IsClient || IsServer;
        public static bool IsClient => NetworkClient.Instance?.Connected ?? false;
        public static bool IsServer => NetworkServer.Instance?.Running ?? false;

        private static int Counter = 0;
        private static IList<PacketWaiter> WaitList = new List<PacketWaiter>();

        public const int ServerPlayerID = 1;
        
        public static void ProcessPacket(Packet packet, int playerId)
        {
            if (packet == null)
                return;

            if (packet is PlayerWelcomePacket wel)
            {
                IGConsole.Log("Your player ID is " + wel.YourID);

                NetworkClient.Instance.SetID(wel.YourID);
            }

            if (packet.SenderID == playerId)
                return;

            foreach (var item in WaitList)
            {
                if (item.Predicate(packet))
                {
                    item.Mre.Set();
                    item.Packet = packet;

                    WaitList.Remove(item);

                    if (!item.ContinueProcessing)
                        return;
                }
            }
            
            switch (packet)
            {
                case PlayerStatePacket state:
                    if (state.PlayerID != playerId)
                        PlayerManager.UpdatePlayer(state);

                    break;
                case TransferDataPacket transfer:
                    Transfer.ReceivePacket(transfer);

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
                        NetworkServer.Instance.SendWorld(NetworkServer.Instance.GetPlayer(signal.SenderID));

                    break;
                case SignalData.Ping:
                    Network.SendPacket(new SignalPacket(SignalData.Pong));

                    break;
                case SignalData.BeginTransfer:
                    Transfer.BeginReceive();

                    break;
                case SignalData.EndTransfer:
                    Transfer.EndReceive(null);

                    break;
                case SignalData.AckTransfer:
                    if (Transfer.IsSending)
                        Transfer.ContinueEvent.Set();

                    break;
            }
        }

        public static Packet WaitForPacket(Predicate<Packet> predicate, bool continueProcessing = false)
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            var item = new PacketWaiter
            {
                Predicate = predicate,
                Mre = mre,
                ContinueProcessing = continueProcessing
            };

            WaitList.Add(item);

            mre.WaitOne();

            return item.Packet;
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

                    if (Counter++ % 120 == 0)
                        IGConsole.Log("Update pos");
                }
            }).Start();
        }

        public static void SendPacket(Packet packet)
        {
#if DEBUG
            //if (!(packet is PlayerStatePacket))
            //    IGConsole.Log($"Send packet of type {packet.GetType().Name} at {Time.time:0.0}");
#endif

            if (IsClient)
                NetworkClient.Instance.Send(packet);
            else if (IsServer)
                NetworkServer.Instance.Broadcast(packet, ServerPlayerID);
        }
    }
}

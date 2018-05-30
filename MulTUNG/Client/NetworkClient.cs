﻿using MulTUNG.Packets;
using System;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;
using Lidgren.Network;
using MulTUNG.Utils;
using MulTUNG.Packets.Utils;
using Server;
using PiTung;

namespace MulTUNG
{
    public class NetworkClient : ISender
    {
        public static NetworkClient Instance { get; private set; } = new NetworkClient();
        
        public int PlayerID { get; private set; } = -2;
        public string Username { get; private set; }
        public bool Connected => Client?.ConnectionStatus == NetConnectionStatus.Connected;
        public bool IsInGameplay { get; private set; }

        public EventWaitHandle EnterEvent = new ManualResetEvent(false);

        private NetClient Client;
        private NetConnectionStatus LastStatus;

        public NetworkClient()
        {
            if (Instance != null && Instance != this)
                throw new ArgumentException("An instance of NetworkClient already exists!");

            Instance = this;
        }

        public void Connect(IPEndPoint endPoint)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MulTUNG");
            Client = new NetClient(config);
            Client.Start();
            var conn = Client.Connect(endPoint);
            
            ThreadPool.QueueUserWorkItem(o =>
            {
                var c = o as NetConnection;
                int elapsed = 0;

                while (true)
                {
                    Thread.Sleep(50);
                    elapsed += 50;
                    
                    if (c.Status != NetConnectionStatus.Connected && elapsed >= Constants.WaitForConnection)
                    {
                        Network.IsClient = false;

                        MulTUNG.Status = "Couldn't connect to remote server.";

                        Thread.Sleep(2000);

                        MulTUNG.ShowMainMenuCanvases();
                        MulTUNG.ShowStatusWindow = false;
                        MulTUNG.Status = "";

                        break;
                    }
                    else if (c.Status == NetConnectionStatus.Connected)
                    {
                        Network.IsClient = true;

                        MulTUNG.SynchronizationContext.Send(_ =>
                        {
                            SaveManager.SaveName = MulTUNG.ForbiddenSaveName;
                            World.DeleteSave();

                            SceneManager.LoadScene("gameplay");
                            EverythingHider.HideEverything();
                        }, null);

                        while (ModUtilities.IsOnMainMenu)
                            Thread.Sleep(500);

                        Thread.Sleep(1000);

                        IsInGameplay = true;
                        EnterEvent.Set();

                        InitWorld();

                        break;
                    }
                }
            }, conn);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                NetIncomingMessage msg;
                
                while (Client.Status == NetPeerStatus.Running)
                {
                    msg = Client.WaitMessage(int.MaxValue);

                    if (msg == null)
                        continue;

                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            var packet = PacketDeserializer.DeserializePacket(new MessagePacketReader(msg));
                            
                            if (Network.ProcessPacket(packet, this.PlayerID))
                                PacketLog.LogReceive(packet);

                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Log.WriteLine("Status: " + Client.ConnectionStatus);

                            if (Client.ConnectionStatus == NetConnectionStatus.Disconnected)
                                Disconnect();

                            LastStatus = Client.ConnectionStatus;
                            break;
                    }

                    Client.Recycle(msg);
                }
            });
        }

        public void Connect(string host) => Connect(new IPEndPoint(IPAddress.Parse(host), Constants.Port));

        public void Disconnect(bool force = false)
        {
            Network.IsClient = false;

            if (ModUtilities.IsOnMainMenu || (LastStatus != NetConnectionStatus.Connected && !force))
                return;

            if (Client?.ConnectionStatus == NetConnectionStatus.Connected)
            {
                Client.Disconnect("bye");
            }

            this.PlayerID = -2;
            PlayerManager.Reset();

            EnterEvent.Reset();
            IsInGameplay = false;

            EverythingHider.HideEverything();
            SceneManager.LoadScene("main menu");
        }
        
        public void Send(Packet packet, NetDeliveryMethod delivery = NetDeliveryMethod.ReliableOrdered)
        {
            packet.SenderID = this.PlayerID;

            PacketLog.LogSend(packet);
            Client.SendMessage(packet.GetMessage(Client), delivery);
        }

        public void SetID(int id)
        {
            if (this.PlayerID == -2)
                this.PlayerID = id;

            Log.WriteLine("Your ID: " + id);
        }

        public void SetUsername(string username)
        {
            if (this.Username != null)
                return;

            this.Username = username;
        }

        private void InitWorld()
        {
            MulTUNG.Status = "Loading world...";

            PlayerManager.NewPlayer(Network.ServerPlayerID, Network.ServerUsername);
            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);

            Thread.Sleep(500);
            Send(new SignalPacket(SignalData.RequestWorld));
        }
    }
}

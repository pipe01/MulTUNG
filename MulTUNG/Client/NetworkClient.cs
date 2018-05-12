﻿using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using System;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Lidgren.Network;
using MulTUNG.Utils;
using MulTUNG.Packeting.Packets.Utils;
using System.IO;
using SavedObjects;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Server;

namespace MulTUNG
{
    public class NetworkClient : ISender
    {
        public static NetworkClient Instance { get; private set; } = new NetworkClient();
        
        public int PlayerID { get; private set; } = -2;
        public bool Connected => Client?.ConnectionStatus == NetConnectionStatus.Connected;

        public bool ReceivingWorld { get; private set; }

        private NetClient Client;

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
            Client.Connect(endPoint);

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
                            
                            PacketLog.LogReceive(packet);

                            Network.ProcessPacket(packet, this.PlayerID);
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            IGConsole.Log("Status: " + Client.ConnectionStatus);
                            break;
                    }
                    Client.Recycle(msg);
                }
            });
        }

        public void Connect(string host) => Connect(new IPEndPoint(IPAddress.Parse(host), Constants.Port));

        public void Disconnect()
        {
            if (Client?.ConnectionStatus == NetConnectionStatus.Connected)
            {
                Client.Disconnect("bye");

                this.PlayerID = -2;
                
                EverythingHider.HideEverything();
                SceneManager.LoadScene("main menu");
            }
        }
        
        public void Send(Packet packet, NetDeliveryMethod delivery = NetDeliveryMethod.ReliableOrdered)
        {
            packet.SenderID = this.PlayerID;
            packet.Time = Time.time;

            PacketLog.LogSend(packet);
            Client.SendMessage(packet.GetMessage(Client), delivery);
        }

        public void SetID(int id)
        {
            if (this.PlayerID == -2)
                this.PlayerID = id;

            IGConsole.Log("Your ID: " + id);

            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);

            Send(new SignalPacket(SignalData.RequestWorld));
        }
    }
}

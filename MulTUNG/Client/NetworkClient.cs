using MulTUNG.Packeting.Packets;
using System;
using System.Net;
using System.Threading;
using UnityEngine.SceneManagement;
using Lidgren.Network;
using MulTUNG.Utils;
using MulTUNG.Packeting.Packets.Utils;
using Server;
using PiTung.Console;
using PiTung;

namespace MulTUNG
{
    public class NetworkClient : ISender
    {
        public static NetworkClient Instance { get; private set; } = new NetworkClient();
        
        public int PlayerID { get; private set; } = -2;
        public string Username { get; private set; }
        public bool Connected => Client?.ConnectionStatus == NetConnectionStatus.Connected;
        
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

                Thread.Sleep(Constants.WaitForConnection);

                if (c.Status != NetConnectionStatus.Connected)
                {
                    IGConsole.Error("Couldn't connect to remote server!");
                    Disconnect(true);
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

                            if (packet.SenderID == this.PlayerID)
                                break;

                            PacketLog.LogReceive(packet);

                            Network.ProcessPacket(packet, this.PlayerID);
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
            if (ModUtilities.IsOnMainMenu || (LastStatus != NetConnectionStatus.Connected && !force))
                return;

            if (Client?.ConnectionStatus == NetConnectionStatus.Connected)
            {
                Client.Disconnect("bye");
            }

            this.PlayerID = -2;
            PlayerManager.Reset();

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

            PlayerManager.NewPlayer(Network.ServerPlayerID, Network.ServerUsername);
            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);

            Send(new SignalPacket(SignalData.RequestWorld));
        }

        public void SetUsername(string username)
        {
            if (this.Username != null)
                return;

            this.Username = username;
        }
    }
}

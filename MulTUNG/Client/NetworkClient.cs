using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using PiTung;
using PiTung.Console;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace MulTUNG
{
    public class NetworkClient : ISender
    {
        public static NetworkClient Instance { get; private set; }

        public TcpClient Client { get; private set; }
        public int PlayerID { get; private set; } = -2;
        public bool Connected => Client?.Connected ?? false;

        public bool ReceivingWorld { get; private set; }

        private BlockingQueue<Packet> SendQueue = new BlockingQueue<Packet>();

        public NetworkClient()
        {
            if (Instance != null)
                throw new ArgumentException("An instance of NetworkClient already exists!");

            Instance = this;
        }

        public void Connect(IPEndPoint endPoint)
        {
            Disconnect();
            
            Client = new TcpClient();

            IGConsole.Log("Connecting...");

            new Thread(() =>
            {
                Client.Connect(endPoint);

                if (Client.Connected)
                {
                    BeginReceive();
                    
                    //Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);
                    
                    StartSending();
                }
            }).Start();
        }

        public void Connect(string host) => Connect(new IPEndPoint(IPAddress.Parse(host), Constants.Port));

        public void Disconnect()
        {
            if (Client?.Client != null && Client.Connected)
            {
                Network.SendPacket(new PlayerDisconnectPacket { PlayerID = this.PlayerID });
                this.PlayerID = -2;

                Client.Close();

                EverythingHider.HideEverything();
                SceneManager.LoadScene("main menu");
            }
        }
        
        public void Send(Packet packet)
        {
            packet.SenderID = this.PlayerID;
            packet.Time = Time.time;

            SendQueue.Enqueue(packet);
            //Client.Client.Send(packet.Serialize());
        }

        public void SetID(int id)
        {
            if (this.PlayerID == -2)
                this.PlayerID = id;


            IGConsole.Log("Request world");
            var pack = new SignalPacket(Packeting.Packets.Utils.SignalData.RequestWorld);

            Send(pack);
        }
        
        public void EndReceivingWorld()
        {
            IGConsole.Log("End receive world");

            ReceivingWorld = false;

            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);
        }

        private void StartSending()
        {
            while (Client.Connected)
            {
                var packet = SendQueue.Dequeue();

                try
                {
                    Client.Client.Send(packet.Serialize());
                }
                catch (Exception ex)
                {
                    IGConsole.Log(ex);
                }
            }
        }

        private void BeginReceive()
        {
            var state = new NetState();

            Client.Client.BeginReceive(state.Buffer, 0, NetState.BufferSize, SocketFlags.None, Received, state);
        }

        private void Received(IAsyncResult ar)
        {
            int rec = 0;

            try
            {
                rec = Client.Client.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                return;
            }

            BeginReceive();

            if (rec == 0)
                return;

            var state = ar.AsyncState as NetState;
            
            var packet = PacketDeserializer.DeserializePacket(state.Buffer);
            Network.ProcessPacket(packet, this.PlayerID);
        }
    }
}

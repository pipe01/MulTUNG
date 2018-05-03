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

namespace MulTUNG
{
    internal class NetworkClient
    {
        public static NetworkClient Instance { get; private set; }

        public TcpClient Client { get; private set; }
        public int PlayerID { get; private set; } = -2;
        public int UpdateInterval { get; set; } = 25;
        public bool Connected => Client?.Connected ?? false;

        private BlockingQueue<Packet> SendQueue = new BlockingQueue<Packet>();
        private CustomFixedUpdate OriginalFixedUpdate = null;

        public NetworkClient()
        {
            Instance = this;
        }

        public void Connect(IPEndPoint endPoint)
        {
            Disconnect();

            PatchCircuitUpdater();

            Client = new TcpClient();

            IGConsole.Log("Connecting...");

            new Thread(() =>
            {
                Client.Connect(endPoint);

                if (Client.Connected)
                {
                    BeginReceive();

                    Network.StartPositionUpdateThread(UpdateInterval);

                    StartSending();
                }
            }).Start();
        }

        public void Connect(string host) => Connect(new IPEndPoint(IPAddress.Parse(host), Constants.Port));

        public void Disconnect()
        {
            if (Client?.Client != null && Client.Connected)
            {
                this.PlayerID = -2;

                Client.Close();

                EverythingHider.HideEverything();
                SceneManager.LoadScene("main menu");
            }
        }

        public void SetID(int id)
        {
            if (this.PlayerID == -2)
                this.PlayerID = id;
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
            var rec = Client.Client.EndReceive(ar);
            BeginReceive();

            var state = ar.AsyncState as NetState;
            
            var packet = PacketDeserializer.DeserializePacket(state.Buffer);
            Network.ProcessPacket(packet, this.PlayerID);
        }
        
        public void SendPacket(Packet packet)
        {
            packet.SenderID = this.PlayerID;
            packet.Time = Time.time;

            SendQueue.Enqueue(packet);
        }

        private void PatchCircuitUpdater(bool restore = false)
        {
            var behaviorManager = GameObject.FindObjectOfType<BehaviorManager>();
            CustomFixedUpdate newUpdater = null;

            if (restore)
            {
                newUpdater = OriginalFixedUpdate;
            }
            else
            {
                newUpdater = new MyFixedUpdate(_ => { });
            }

            ModUtilities.SetFieldValue(behaviorManager, "CircuitLogicUpdate", newUpdater);
        }
    }
}

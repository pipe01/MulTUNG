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
        public TcpClient Client { get; private set; }
        public int PlayerID { get; private set; } = -2;
        public int UpdateInterval { get; set; } = 25;

        private BlockingQueue<Packet> SendQueue = new BlockingQueue<Packet>();
        private CustomFixedUpdate OriginalFixedUpdate = null;

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

                    new Thread(UpdatePlayerStateLoop).Start();

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

        private void UpdatePlayerStateLoop()
        {
            while (Client.Connected)
            {
                Thread.Sleep(UpdateInterval);

                if (ModUtilities.IsOnMainMenu)
                    continue;
                
                SendPacket(new PlayerStatePacket
                {
                    PlayerID = this.PlayerID,
                    Time = Time.time,
                    Position = FirstPersonInteraction.FirstPersonCamera.transform.position,
                    EulerAngles = FirstPersonInteraction.FirstPersonCamera.transform.eulerAngles
                });
            }
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
            PacketProcessor.Process(packet);
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

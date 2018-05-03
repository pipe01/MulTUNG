using Client.Packeting;
using Client.Packeting.Packets;
using PiTung;
using PiTung.Console;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client
{
    internal class NetworkClient
    {
        public TcpClient Client { get; private set; }
        public int PlayerID { get; private set; } = -2;
        public int UpdateInterval { get; set; } = 25;

        private BlockingQueue<Packet> SendQueue = new BlockingQueue<Packet>();
        private CustomFixedUpdate OriginalFixedUpdate = null;

        public void Connect(string host)
        {
            Disconnect();

            PatchCircuitUpdater();

            Client = new TcpClient();

            IGConsole.Log("Connecting...");

            new Thread(() =>
            {
                Client.Connect(host, Constants.Port);

                if (Client.Connected)
                {
                    BeginReceive();

                    new Thread(UpdatePlayerStateLoop).Start();

                    StartSending();
                }
            }).Start();
        }

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
            HandlePacket(packet);
        }

        private void HandlePacket(Packet packet)
        {
            if (packet is PlayerWelcomePacket wel)
            {
                IGConsole.Log("Your player ID is " + wel.YourID);

                this.PlayerID = wel.YourID;
            }
            if (this.PlayerID == -2 || packet.SenderID == this.PlayerID)
            {
                return;
            }

            switch (packet)
            {
                case PlayerStatePacket state:
                    if (state.PlayerID != this.PlayerID)
                    {
                        PlayerManager.UpdatePlayer(state);
                    }

                    break;
                case PlaceBoardPacket board:
                    if (board.AuthorID != this.PlayerID)
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

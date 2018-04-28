using Common;
using Common.Packets;
using PiTung;
using PiTung.Console;
using System;
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
        public int UpdateInterval { get; set; } = 500;

        private BlockingQueue<Packet> SendQueue = new BlockingQueue<Packet>();

        public void Connect(string host)
        {
            Disconnect();

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
                
                Client.Client.Send(packet.Serialize());
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
            //IGConsole.Log(packet.SenderID + ": " + packet.GetType().Name);

            if (packet is PlayerWelcomePacket wel)
            {
                IGConsole.Log("Your player ID is " + wel.YourID);

                this.PlayerID = wel.YourID;
            }
            else if (this.PlayerID != -2)
            {
                if (packet is PlayerStatePacket state)
                {
                    if (state.PlayerID != this.PlayerID)
                        PlayerManager.UpdatePlayer(state);
                }
                else if (packet is PlaceBoardPacket board)
                {
                    if (board.AuthorID != this.PlayerID)
                        NetUtilitiesComponent.Instance.Enqueue(new PlaceBoardJob(board));
                }
                else if (packet is DeleteBoardPacket del)
                {
                    NetUtilitiesComponent.Instance.Enqueue(new DeleteBoardJob(del.BoardID));
                }
            }
        }

        public void SendPacket(Packet packet)
        {
            packet.SenderID = this.PlayerID;

            SendQueue.Enqueue(packet);
        }

        public void UpdatePosition(Transform playerTransform)
        {
            SendPacket(new PlayerStatePacket
            {
                Time = Time.time,
                PlayerID = PlayerID,
                EulerAngles = playerTransform.eulerAngles,
                Position = playerTransform.position
            });
        }
    }
}

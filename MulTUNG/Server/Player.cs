using MulTUNG;
using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using PiTung.Console;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Server
{
    internal class Player : ISender
    {
        public TcpClient Client { get; }
        public int ID { get; }
        public float LastUpdateTime { get; private set; }

        public bool Connected => Client?.Client != null && Client.Connected;

        private ManualResetEvent PingMre = new ManualResetEvent(false);
        private int FailedPings = 0;

        public Player(TcpClient client, int id)
        {
            this.Client = client;
            this.ID = id;

            BeginPing();
            BeginReceive();
        }

        public void Send(Packet packet)
        {
            if (!Connected)
                return;

            MyDebug.Log($"Send to player {ID}: {packet.GetType().Name}" + (packet is SignalPacket signal ? $" ({signal.Data.ToString()})" : ""));

            Client.Client.Send(packet.Serialize());
        }

        public void Disconnect()
        {
            if (Connected)
            {
                try
                {
                    NetworkServer.Instance.Broadcast(new PlayerDisconnectPacket
                    {
                        PlayerID = this.ID
                    }, this.ID);
                }
                catch { }
                
                Client.Close();

                Log.WriteLine($"Player {this.ID} disconnected");
            }
        }

        private void BeginPing()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (Connected)
                {
                    Thread.Sleep(Constants.PingInterval);

                    Send(new SignalPacket(SignalData.Ping));

                    bool received = PingMre.WaitOne(Constants.PingTimeout);
                    PingMre.Reset();

                    if (!received)
                    {
                        FailedPings++;

                        if (FailedPings == Constants.MaxFailedPings - 1)
                        {
                            Log.WriteLine("Dropping player " + this.ID);
                            Disconnect();
                        }
                    }
                    else
                    {
                        FailedPings = 0;
                    }
                }
            });
        }

        private void BeginReceive()
        {
            var state = new NetState();

            Client.Client.BeginReceive(state.Buffer, 0, NetState.BufferSize, SocketFlags.None, Received, state);

            void Received(IAsyncResult ar)
            {
                int size = 0;

                try
                {
                    size = Client.Client.EndReceive(ar);
                }
                catch { }

                if (size == 0)
                {
                    Disconnect();
                    return;
                }

                var st = ar.AsyncState as NetState;

                BeginReceive();

                var packet = PacketDeserializer.DeserializePacket(st.Buffer, out int _); //TODO Correct packet reading

                if (packet == null)
                    return;

                if (packet is PlayerStatePacket)
                    LastUpdateTime = Time.time;
                else if (packet is SignalPacket signal)
                {
                    if (signal.Data == SignalData.Pong)
                        PingMre.Set();
                    else
                        MulTUNG.Network.HandleSignalPacket(signal);
                }

                if (packet.ShouldBroadcast)
                {
                    NetworkServer.Instance.Broadcast(packet, packet.SenderID);
                }
            }
        }
    }
}

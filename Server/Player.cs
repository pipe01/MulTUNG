using Common;
using Common.Packets;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    internal class Player
    {
        public TcpClient Client { get; }
        public int ID { get; }

        public bool Connected => Client?.Client != null && Client.Connected;

        public Player(TcpClient client, int id)
        {
            this.Client = client;
            this.ID = id;

            BeginReceive();
        }

        public void Send(Packet packet)
        {
            if (!Connected)
                return;
            
            Client.Client.Send(packet.Serialize());
        }

        public void Disconnect()
        {
            if (Connected)
            {
                try
                {
                    NetworkServer.Instance.Broadcast(new PlayerStatePacket
                    {
                        Connected = false,
                        PlayerID = this.ID
                    }, this.ID);
                }
                catch { }
                
                Client.Close();

                Log.WriteLine($"Player {this.ID} disconnected");
            }
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

                var packet = PacketDeserializer.DeserializePacket(st.Buffer);

                if (packet == null)
                    return;

                if (!(packet is PlayerStatePacket))
                {
                    Log.WriteLine($"Packet of type {packet.GetType().Name} received from {packet.SenderID}");
                }
                
                if (packet.ShouldBroadcast)
                {
                    NetworkServer.Instance.Broadcast(packet, packet.SenderID);
                }
            }
        }
    }
}

using Common;
using Common.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class NetworkServer
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();
        public static NetworkServer Instance { get; private set; }

        private int PlayerIdCounter = 123;
        private TcpListener Listener;
        private IList<Player> Players = new List<Player>();

        public NetworkServer()
        {
            Instance = this;
        }

        public void Start()
        {
            Listener = new TcpListener(IPAddress.Any, Constants.Port);
            Listener.Start();

            BeginAcceptTcpClient();
        }

        public void Broadcast(Packet packet, params int[] excludeIds)
        {
            Broadcast(packet.Serialize(), excludeIds);
        }

        public void Broadcast(byte[] data, params int[] excludeIds)
        {
            Parallel.ForEach(Players, o =>
            {
                if (excludeIds.Contains(o.ID))
                    return;

                try
                {
                    o.Client.Client.Send(data);
                }
                catch
                {
                    o.Disconnect();
                }
            });
        }

        private void BeginAcceptTcpClient()
        {
            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);

            void AcceptTcpClient(IAsyncResult ar)
            {
                var client = Listener.EndAcceptTcpClient(ar);

                BeginAcceptTcpClient();

                Log.WriteLine($"New player with ID {PlayerIdCounter} connected.");

                var player = new Player(client, PlayerIdCounter++);
                player.Send(new PlayerWelcomePacket { YourID = player.ID, SenderID = -1 });
                
                Players.Add(player);
            }
        }
    }
}

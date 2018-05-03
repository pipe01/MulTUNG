using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Server
{
    internal class NetworkServer
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();
        public static NetworkServer Instance { get; private set; }

        private int _CircuitUpdateRate;
        public int CircuitUpdateRate
        {
            get => _CircuitUpdateRate;
            set
            {
                _CircuitUpdateRate = value;

                CircuitUpdateTime = TimeSpan.FromMilliseconds(1000f / value);
            }
        }

        public IPEndPoint LocalEndPoint => Listener?.LocalEndpoint as IPEndPoint;

        private TimeSpan CircuitUpdateTime;
        private int PlayerIdCounter = 123;
        private TcpListener Listener;
        private IList<Player> Players = new List<Player>();

        public NetworkServer()
        {
            Instance = this;
            CircuitUpdateRate = 100;
        }

        public void Start()
        {
            Listener = new TcpListener(IPAddress.Any, Constants.Port);
            Listener.Start();
            
            StartCircuitUpdateClock();

            BeginAcceptTcpClient();

            Log.WriteLine("Listening on port " + Constants.Port);
        }

        public void Broadcast(Packet packet, params int[] excludeIds)
        {
            Broadcast(packet.Serialize(), excludeIds);
        }

        public void Broadcast(byte[] data, params int[] excludeIds)
        {
            foreach (var item in Players.Where(o => !excludeIds.Contains(o.ID)))
            {
                try
                {
                    item.Client.Client.Send(data);
                }
                catch { }
            }
        }

        private void StartCircuitUpdateClock()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(CircuitUpdateTime);
                    
                    Broadcast(new CircuitUpdatePacket());
                }
            }).Start();
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

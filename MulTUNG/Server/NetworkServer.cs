using MulTUNG;
using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using MulTUNG.Utils;
using PiTung.Console;
using SavedObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;
using Network = MulTUNG.Network;

namespace Server
{
    internal class NetworkServer : ISender
    {
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

        public bool Running => Listener != null;

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

            LoadWorld();

            StartCircuitUpdateClock();
            StartPlayerClock();

            BeginAcceptTcpClient();

            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);

            Log.WriteLine("Listening on port " + Constants.Port);
        }

        public void Send(Packet packet) => Broadcast(packet.Serialize());

        public void Broadcast(Packet packet, params int[] excludeIds)
        {
            Network.ProcessPacket(packet, 0);

            if (!(packet is PlayerStatePacket))
                MyDebug.Log($"Server broadcast: {packet.GetType().Name}" + (packet is SignalPacket signal ? $" ({signal.Data.ToString()})" : ""));

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

        public Player GetPlayer(int id) => Players.SingleOrDefault(o => o.ID == id);

        public void LoadWorld()
        {
            foreach (var item in GameObject.FindObjectsOfType<ObjectInfo>())
            {
                if (item.GetComponent<NetObject>() == null)
                    item.gameObject.AddComponent<NetObject>().NetID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
        }

        public void SendWorld(Player player)
        {
            IGConsole.Log("Sending world to player");

            List<SavedObjectV2> topLevelObjects = SaveManager.GetTopLevelObjects();

            BinaryFormatter bin = new BinaryFormatter();

            using (MemoryStream mem = new MemoryStream())
            {
                bin.Serialize(mem, topLevelObjects);

                mem.Position = 0;

                //TODO Add a Semaphore in order to prevent sending the world to multiple players at once
                Transfer.Send(mem, player);
            }
        }

        private void StartCircuitUpdateClock()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(CircuitUpdateTime);
                    
                    //Broadcast(new SignalPacket(MulTUNG.Packeting.Packets.Utils.SignalData.CircuitUpdate));
                }
            }).Start();
        }

        private void StartPlayerClock()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    foreach (var item in Players)
                    {
                        if (Time.time - item.LastUpdateTime > Constants.MaximumPlayerStateTime)
                        {
                            //IGConsole.Log($"Player {item.ID} hasn't updated in a while!");
                        }
                    }
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

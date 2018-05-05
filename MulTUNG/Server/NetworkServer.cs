using MulTUNG;
using MulTUNG.Packeting;
using MulTUNG.Packeting.Packets;
using MulTUNG.Packeting.Packets.Utils;
using MulTUNG.Utils;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Network = MulTUNG.Network;

namespace Server
{
    internal class NetworkServer
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

        public void Broadcast(Packet packet, params int[] excludeIds)
        {
            Network.ProcessPacket(packet, 0);

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

            var allObjs = GameObject.FindObjectsOfType<ObjectInfo>();
            var boards = allObjs
                .Where(o => o.ComponentType == ComponentType.CircuitBoard).ToList()
                .PushToTop(o => o.transform.parent == null);

            IGConsole.Log("Parents: " + string.Join("; ", boards.Select(o => o.transform.parent == null ? "Yes" : "No").ToArray()));

            foreach (var item in 
                boards.Concat(
                allObjs.Where(o => o.ComponentType != ComponentType.Wire && o.ComponentType != ComponentType.CircuitBoard).Concat(
                allObjs.Where(o => o.ComponentType == ComponentType.Wire))))
            {
                var netObj = item.GetComponent<NetObject>();

                if (netObj == null)
                    continue;

                var type = item.ComponentType;

                if (type == ComponentType.CircuitBoard)
                {
                    var board = item.GetComponent<CircuitBoard>();

                    player.Send(new PlaceBoardPacket
                    {
                        BoardID = netObj.NetID,
                        Width = board.x,
                        Height = board.z,
                        Position = item.transform.position,
                        EulerAngles = item.transform.eulerAngles,
                        ParentBoardID = item.transform.parent?.GetComponent<NetObject>()?.NetID ?? 0
                    });
                }
                else if (type == ComponentType.Wire)
                {
                    var wire = item.GetComponent<Wire>();

                    var packet = PlaceWirePacket.BuildFromLocalWire(wire);

                    if (packet != null)
                        player.Send(packet);
                }
                else
                {
                    player.Send(PlaceComponentPacket.BuildFromLocalComponent(item.gameObject));
                }
            }

            player.Send(new SignalPacket(SignalData.WorldEnd));
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

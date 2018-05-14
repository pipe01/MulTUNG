using Lidgren.Network;
using MulTUNG;
using MulTUNG.Packeting.Packets;
using MulTUNG.Server;
using MulTUNG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Network = MulTUNG.Network;

namespace Server
{
    public class NetworkServer : ISender
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

        public bool Running => Server?.Status == NetPeerStatus.Running;

        public IPEndPoint LocalEndPoint => null;//Listener?.LocalEndpoint as IPEndPoint;

        private TimeSpan CircuitUpdateTime;
        private int PlayerIdCounter = 123;

        private NetServer Server;
        private IDictionary<int, Player> Players = new Dictionary<int, Player>();

        public NetworkServer()
        {
            Instance = this;
            CircuitUpdateRate = 100;
        }

        public void Start()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MulTUNG");
            config.Port = Constants.Port;

            Server = new NetServer(config);
            Server.Start();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                NetIncomingMessage msg;
                while (Server != null)
                {
                    msg = Server.WaitMessage(int.MaxValue);

                    if (msg == null)
                        continue;

                    HandleMessage(msg);

                    Server.Recycle(msg);
                }
            });

            World.AddNetObjects();
            World.Serialize();

            //StartCircuitUpdateClock();

            Network.StartPositionUpdateThread(Constants.PositionUpdateInterval);

            Log.WriteLine("Listening on port " + Constants.Port);
        }

        private void HandleMessage(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.Data:
                    var packet = PacketDeserializer.DeserializePacket(new MessagePacketReader(msg));

                    if (packet.SenderID == Network.ServerPlayerID)
                        break;

                    PacketLog.LogReceive(packet);

                    if (packet.ShouldBroadcast)
                        Broadcast(packet, packet.ReliableBroadcast ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced);
                    else
                        Network.ProcessPacket(packet, Network.ServerPlayerID);

                    break;
                case NetIncomingMessageType.StatusChanged:
                    var status = (NetConnectionStatus)msg.ReadByte();

                    if (status == NetConnectionStatus.Connected)
                    {

                        int id = PlayerIdCounter++;

                        msg.SenderConnection.SendMessage(new PlayerWelcomePacket
                        {
                            YourID = id
                        }.GetMessage(Server), NetDeliveryMethod.ReliableOrdered, 0);

                        var player = new Player(id, msg.SenderConnection);
                        
                        Log.WriteLine("Connected player " + player.ID);

                        PlayerManager.NewPlayer(player.ID);
                        Players.Add(id, player);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        int id = Players.Keys.SingleOrDefault(o => Players[o].Connection == msg.SenderConnection);

                        PlayerManager.WaveGoodbye(id);
                    }

                    break;
            }
        }

        public void SendStatesToPlayers()
        {
            var packet = new StateListPacket();

            packet.States.Add(Network.ServerPlayerID, new PlayerState
            {
                PlayerID = Network.ServerPlayerID,
                Position = FirstPersonInteraction.FirstPersonCamera.transform.position,
                EulerAngles = FirstPersonInteraction.FirstPersonCamera.transform.eulerAngles
            });

            foreach (var item in Players)
            {
                packet.States.Add(item.Value.ID, new PlayerState
                {
                    PlayerID = item.Value.ID,
                    Position = item.Value.Position,
                    EulerAngles = item.Value.EulerAngles
                });
            }

            Broadcast(packet, NetDeliveryMethod.ReliableSequenced);
        }

        public void UpdatePlayerState(PlayerStatePacket packet)
        {
            if (Players.TryGetValue(packet.PlayerID, out var player))
            {
                player.Position = packet.Position;
                player.EulerAngles = packet.EulerAngles;
            }
        }

        public void Send(Packet packet, NetDeliveryMethod delivery = NetDeliveryMethod.ReliableOrdered) => Broadcast(packet, delivery);

        public void Broadcast(Packet packet, NetDeliveryMethod delivery)
        {
            Network.ProcessPacket(packet, 0);

            var msg = packet.GetMessage(Server);
            
            Server.SendToAll(msg, delivery);

            PacketLog.LogSend(packet);
        }

        public void SendWorld(int playerId)
        {
            var player = Players[playerId];

            Log.WriteLine("Sending world to player " + playerId);

            byte[] world = World.Serialize();

            var msg = new WorldDataPacket
            {
                Data = world
            }.GetMessage(Server);

            player.Connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
        }
    }
}

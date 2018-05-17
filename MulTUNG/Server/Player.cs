using Lidgren.Network;
using UnityEngine;

namespace MulTUNG.Server
{
    public class Player
    {
        public int ID { get; }
        public string Username { get; set; }
        public NetConnection Connection { get; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }

        public Player(int id, NetConnection connection)
        {
            this.ID = id;
            this.Connection = connection;
        }
    }
}

using Lidgren.Network;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace MulTUNG.Server
{
    [Serializable]
    public class Player : ISerializable
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

        public Player(SerializationInfo info, StreamingContext context)
        {
            this.ID = info.GetInt32(nameof(ID));
            this.Username = info.GetString(nameof(Username));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ID), ID);
            info.AddValue(nameof(Username), Username);
        }
    }
}

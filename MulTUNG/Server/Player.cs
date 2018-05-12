using Lidgren.Network;
using MulTUNG.Packeting.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Server
{
    public class Player
    {
        public int ID { get; }
        //public GameObject Object { get; }
        public NetConnection Connection { get; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }

        public Player(int id, /*GameObject @object,*/ NetConnection connection)
        {
            this.ID = id;
            //this.Object = @object;
            this.Connection = connection;
        }
    }
}

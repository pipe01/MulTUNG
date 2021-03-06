﻿using Lidgren.Network;
using PiTung.Console;
using System;

namespace MulTUNG.Packets
{
    [Serializable]
    public abstract class Packet
    {
        public virtual bool ShouldBroadcast => false;
        public virtual bool ReliableBroadcast => true;
        public virtual bool ReceiveOwn => false;

        public byte[] Serialize()
        {
            return new PacketBuilder()
                .Write(Type)
                .Write(Time)
                .Write(SenderID)
                .WriteRaw(SerializeInner())
                .Done();
        }

        protected abstract byte[] SerializeInner();

        public abstract void Deserialize(IReader reader);

        public abstract PacketType Type { get; }

        public float Time { get; set; }
        public int SenderID { get; set; }

        public NetOutgoingMessage GetMessage(NetPeer peer)
        {
            var msg = peer.CreateMessage();
            try
            {
                msg.Write(this.Serialize());
            }
            catch (Exception ex)
            {
                IGConsole.Log(ex);
            }
            return msg;
        }
    }
}

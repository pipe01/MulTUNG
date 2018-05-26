using Lidgren.Network;
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
        
        public abstract PacketType Type { get; }

        public float Time { get; set; }
        public int SenderID { get; set; }

        public NetOutgoingMessage GetMessage(NetPeer peer)
        {
            var msg = peer.CreateMessage();
            msg.Write(this.Serialize());
            return msg;
        }
    }
}

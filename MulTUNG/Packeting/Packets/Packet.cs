using System;

namespace MulTUNG.Packeting.Packets
{
    [Serializable]
    public abstract class Packet
    {
        public virtual bool ShouldBroadcast => false;

        public byte[] Serialize()
        {
            return new PacketBuilder()
                .WritePacketType(Type)
                .WriteFloat(Time)
                .WriteInt32(SenderID)
                .Write(SerializeInner())
                .Done();
        }

        protected abstract byte[] SerializeInner();
        
        public abstract PacketType Type { get; }

        public float Time { get; set; }
        public int SenderID { get; set; }
    }
}

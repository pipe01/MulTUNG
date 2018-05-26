using System;

namespace MulTUNG.Packets
{
    public class DeleteComponentPacket : Packet
    {
        public override PacketType Type => PacketType.DeleteComponent;
        public override bool ShouldBroadcast => true;

        public int ComponentNetID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(ComponentNetID);
        }

        public override void Deserialize(IReader reader)
        {
            this.ComponentNetID = reader.ReadInt32();
        }
    }
}

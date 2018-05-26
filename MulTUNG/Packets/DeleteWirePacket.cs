using System;

namespace MulTUNG.Packets
{
    public class DeleteWirePacket : Packet
    {
        public override PacketType Type => PacketType.DeleteWire;
        public override bool ShouldBroadcast => true;

        public int WireNetID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(WireNetID);
        }

        public override void Deserialize(IReader reader)
        {
            this.WireNetID = reader.ReadInt32();
        }
    }
}

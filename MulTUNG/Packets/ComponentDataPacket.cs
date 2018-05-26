using System.Collections.Generic;

namespace MulTUNG.Packets
{
    public class ComponentDataPacket : Packet
    {
        public override PacketType Type => PacketType.ComponentData;
        public override bool ShouldBroadcast => true;

        public int NetID { get; set; }
        public ComponentType ComponentType { get; set; }
        public List<object> Data { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(NetID)
                .Write((int)ComponentType)
                .WriteBinaryObject(Data)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.NetID = reader.ReadInt32();
            this.ComponentType = (ComponentType)reader.ReadInt32();
            this.Data = reader.ReadBinaryObject<List<object>>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
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

        public static ComponentDataPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<ComponentDataPacket>();
            packet.NetID = reader.ReadInt32();
            packet.ComponentType = (ComponentType)reader.ReadInt32();
            packet.Data = reader.ReadBinaryObject<List<object>>();

            return packet;
        }
    }
}

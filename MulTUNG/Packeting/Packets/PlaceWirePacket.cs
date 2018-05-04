using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class PlaceWirePacket : Packet
    {
        public override PacketType Type => PacketType.PlaceWire;
        public override bool ShouldBroadcast => true;

        public int NetObj1Id { get; set; }
        public int Point1Id { get; set; }
        public int NetObj2Id { get; set; }
        public int Point2Id { get; set; }
        public int NetID { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(NetObj1Id)
                .WriteInt32(Point1Id)
                .WriteInt32(NetObj2Id)
                .WriteInt32(Point2Id)
                .WriteInt32(NetID)
                .Done();
        }

        public static PlaceWirePacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlaceWirePacket>();
            packet.NetObj1Id = reader.ReadInt32();
            packet.Point1Id = reader.ReadInt32();
            packet.NetObj2Id = reader.ReadInt32();
            packet.Point2Id = reader.ReadInt32();
            packet.NetID = reader.ReadInt32();

            return packet;
        }
    }
}

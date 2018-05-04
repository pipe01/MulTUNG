using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
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

        public static DeleteWirePacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<DeleteWirePacket>();
            packet.WireNetID = reader.ReadInt32();

            return packet;
        }
    }
}

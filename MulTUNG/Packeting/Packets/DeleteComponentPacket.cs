using System;
using System.Collections.Generic;
using System.Text;

namespace MulTUNG.Packeting.Packets
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

        public static DeleteComponentPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<DeleteComponentPacket>();
            packet.ComponentNetID = reader.ReadInt32();

            return packet;
        }
    }
}

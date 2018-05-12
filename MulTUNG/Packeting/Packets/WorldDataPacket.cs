using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class WorldDataPacket : Packet
    {
        public override PacketType Type => PacketType.WorldData;

        public byte[] Data { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Data)
                .Done();
        }

        public static WorldDataPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<WorldDataPacket>();
            packet.Data = reader.ReadByteArray();

            return packet;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packets
{
    public class PauseGamePacket : Packet
    {
        public override PacketType Type => PacketType.PauseGame;

        public string Reason { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Reason)
                .Done();
        }

        public static PauseGamePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PauseGamePacket>();
            packet.Reason = reader.ReadString();

            return packet;
        }
    }
}

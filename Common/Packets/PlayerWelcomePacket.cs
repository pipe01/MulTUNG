using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Packets
{
    public class PlayerWelcomePacket : Packet
    {
        public override PacketType Type => PacketType.PlayerWelcome;

        public int YourID { get; set; }
        
        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(YourID)
                .Done();
        }

        public static PlayerWelcomePacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlayerWelcomePacket>();
            packet.YourID = reader.ReadInt32();

            return packet;
        }
    }
}

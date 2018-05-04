using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class PlayerDisconnectPacket : Packet
    {
        public override PacketType Type => PacketType.PlayerDisconnect;

        public int PlayerID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(PlayerID);
        }

        public static PlayerDisconnectPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlayerDisconnectPacket>();
            packet.PlayerID = reader.ReadInt32();

            return packet;
        }
    }
}

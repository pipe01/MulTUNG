using System;

namespace MulTUNG.Packeting.Packets
{
    public class PlayerDisconnectPacket : Packet
    {
        public override PacketType Type => PacketType.PlayerDisconnect;
        public override bool ShouldBroadcast => true;

        public int PlayerID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(PlayerID);
        }

        public static PlayerDisconnectPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlayerDisconnectPacket>();
            packet.PlayerID = reader.ReadInt32();

            return packet;
        }
    }
}

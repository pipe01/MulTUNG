using System;

namespace MulTUNG.Packets
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

        public override void Deserialize(IReader reader)
        {
            this.PlayerID = reader.ReadInt32();
        }
    }
}

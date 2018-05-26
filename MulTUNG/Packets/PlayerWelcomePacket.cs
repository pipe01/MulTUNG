namespace MulTUNG.Packets
{
    public class PlayerWelcomePacket : Packet
    {
        public override PacketType Type => PacketType.PlayerWelcome;

        public int YourID { get; set; }
        public string ServerUsername { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(YourID)
                .Write(ServerUsername)
                .Done();
        }

        public static PlayerWelcomePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlayerWelcomePacket>();
            packet.YourID = reader.ReadInt32();
            packet.ServerUsername = reader.ReadString();

            return packet;
        }
    }
}

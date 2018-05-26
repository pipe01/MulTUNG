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

        public override void Deserialize(IReader reader)
        {
            this.YourID = reader.ReadInt32();
            this.ServerUsername = reader.ReadString();
        }
    }
}

namespace MulTUNG.Packets
{
    public class PlayerDataPacket : Packet
    {
        public override PacketType Type => PacketType.PlayerData;
        public override bool ShouldBroadcast => true;

        public string Username { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Username)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.Username = reader.ReadString();
        }
    }
}

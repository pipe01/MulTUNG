namespace MulTUNG.Packets
{
    public class ChatMessagePacket : Packet
    {
        public override PacketType Type => PacketType.ChatMessage;
        public override bool ShouldBroadcast => true;
        public override bool ReceiveOwn => true;

        public string Username { get; set; }
        public string Text { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Username)
                .Write(Text)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.Username = reader.ReadString();
            this.Text = reader.ReadString();
        }
    }
}

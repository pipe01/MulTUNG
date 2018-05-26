namespace MulTUNG.Packets
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

        public override void Deserialize(IReader reader)
        {
            this.Data = reader.ReadByteArray();
        }
    }
}

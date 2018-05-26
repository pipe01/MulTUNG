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

        public static WorldDataPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<WorldDataPacket>();
            packet.Data = reader.ReadByteArray();

            return packet;
        }
    }
}

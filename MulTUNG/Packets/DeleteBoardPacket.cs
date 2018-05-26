using System;

namespace MulTUNG.Packets
{
    public class DeleteBoardPacket : Packet
    {
        public override PacketType Type => PacketType.DeleteBoard;
        public override bool ShouldBroadcast => true;

        public int BoardID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(BoardID);
        }

        public static DeleteBoardPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<DeleteBoardPacket>();
            packet.BoardID = reader.ReadInt32();

            return packet;
        }
    }
}

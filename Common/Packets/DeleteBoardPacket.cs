using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Packets
{
    public class DeleteBoardPacket : Packet
    {
        public override PacketType Type => PacketType.DeleteBoard;

        public int BoardID { get; set; }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes(BoardID);
        }

        public static DeleteBoardPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<DeleteBoardPacket>();
            packet.BoardID = reader.ReadInt32();

            return packet;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class TransferDataPacket : Packet
    {
        public override PacketType Type => PacketType.TransferData;

        public const int DataBufferSize = 2000;//NetState.BufferSize - 10;

        public int Index { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(Index)
                .WriteInt32(Length)
                .WriteByteArray(Data)
                .Done();
        }

        public static TransferDataPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<TransferDataPacket>();
            packet.Index = reader.ReadInt32();
            packet.Length = reader.ReadInt32();
            packet.Data = reader.ReadByteArray();

            return packet;
        }
    }
}

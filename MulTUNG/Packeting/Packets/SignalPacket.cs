using MulTUNG.Packeting.Packets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class SignalPacket : Packet
    {
        public override PacketType Type => PacketType.Signal;

        public SignalData Data { get; set; }

        public SignalPacket()
        {
        }

        public SignalPacket(SignalData data)
        {
            this.Data = data;
        }

        protected override byte[] SerializeInner()
        {
            return BitConverter.GetBytes((int)Data);
        }

        public static SignalPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<SignalPacket>();
            packet.Data = (SignalData)reader.ReadInt32();

            return packet;
        }
    }
}

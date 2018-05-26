using MulTUNG.Packeting.Packets.Utils;
using System;

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

        public static SignalPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<SignalPacket>();
            packet.Data = (SignalData)reader.ReadInt32();

            return packet;
        }

        public override string ToString() => Data.ToString();
    }
}

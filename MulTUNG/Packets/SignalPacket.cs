using MulTUNG.Packets.Utils;
using System;

namespace MulTUNG.Packets
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

        public override void Deserialize(IReader reader)
        {
            this.Data = (SignalData)reader.ReadInt32();
        }

        public override string ToString() => Data.ToString();
    }
}

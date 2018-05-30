using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packets
{
    public class PauseGamePacket : Packet
    {
        public override PacketType Type => PacketType.PauseGame;

        public string Reason { get; set; }
        public int ExceptID { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Reason)
                .Write(ExceptID)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.Reason = reader.ReadString();
            this.ExceptID = reader.ReadInt32();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class CircuitUpdatePacket : Packet
    {
        public override PacketType Type => PacketType.CircuitUpdate;

        protected override byte[] SerializeInner()
        {
            return new byte[0];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Packets
{
    public class PaintBoardPacket : Packet
    {
        public override PacketType Type => PacketType.PaintBoard;
        public override bool ShouldBroadcast => true;

        public int BoardID { get; set; }
        public Color Color { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(BoardID)
                .Write(Color)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.BoardID = reader.ReadInt32();
            this.Color = reader.ReadColor();
        }
    }
}

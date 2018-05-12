using UnityEngine;

namespace MulTUNG.Packeting.Packets
{
    public class PlaceBoardPacket : Packet
    {
        public override PacketType Type => PacketType.PlaceBoard;
        public override bool ShouldBroadcast => true;

        public int BoardID { get; set; }
        public int ParentBoardID { get; set; }
        public int AuthorID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(BoardID)
                .Write(ParentBoardID)
                .Write(AuthorID)
                .Write(Width)
                .Write(Height)
                .Write(Position)
                .Write(EulerAngles)
                .Done();
        }

        public static PlaceBoardPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlaceBoardPacket>();
            packet.BoardID = reader.ReadInt32();
            packet.ParentBoardID = reader.ReadInt32();
            packet.AuthorID = reader.ReadInt32();
            packet.Width = reader.ReadInt32();
            packet.Height = reader.ReadInt32();
            packet.Position = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();

            return packet;
        }
    }
}

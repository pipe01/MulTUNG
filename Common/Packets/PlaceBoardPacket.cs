using UnityEngine;

namespace Common.Packets
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
                .WriteInt32(BoardID)
                .WriteInt32(ParentBoardID)
                .WriteInt32(AuthorID)
                .WriteInt32(Width)
                .WriteInt32(Height)
                .WriteVector3(Position)
                .WriteVector3(EulerAngles)
                .Done();
        }

        public static PlaceBoardPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

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

using UnityEngine;

namespace MulTUNG.Packets
{
    public class PlayerStatePacket : Packet
    {
        public override PacketType Type => PacketType.PlayerState;
        //public override bool ShouldBroadcast => true;

        public int PlayerID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(PlayerID)
                .Write(Position)
                .Write(EulerAngles)
                .Done();
        }

        public static PlayerStatePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlayerStatePacket>();
            packet.PlayerID = reader.ReadInt32();
            packet.Position = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();
            
            return packet;
        }

        public override string ToString() => Position.ToString();
    }
}

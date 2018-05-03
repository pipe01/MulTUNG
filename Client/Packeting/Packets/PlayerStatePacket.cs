using UnityEngine;

namespace Common.Packets
{
    public class PlayerStatePacket : Packet
    {
        public int PlayerID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 EulerAngles { get; set; }
        public bool Connected { get; set; } = true;

        public override bool ShouldBroadcast => true;
        public override PacketType Type => PacketType.PlayerState;

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(PlayerID)
                .WriteBool(Connected)
                .WriteVector3(Position)
                .WriteVector3(EulerAngles)
                .Done();
        }

        public static PlayerStatePacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlayerStatePacket>();
            packet.PlayerID = reader.ReadInt32();
            packet.Connected = reader.ReadBool();
            packet.Position = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();

            return packet;
        }
    }
}

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

        public override void Deserialize(IReader reader)
        {
            this.PlayerID = reader.ReadInt32();
            this.Position = reader.ReadVector3();
            this.EulerAngles = reader.ReadVector3();
        }

        public override string ToString() => Position.ToString();
    }
}

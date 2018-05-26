using UnityEngine;

namespace MulTUNG.Packets
{
    public class RotateComponentPacket : Packet
    {
        public override PacketType Type => PacketType.RotateComponent;
        public override bool ShouldBroadcast => true;

        public int ComponentID { get; set; }
        public Vector3 EulerAngles { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(ComponentID)
                .Write(EulerAngles)
                .Done();
        }

        public override void Deserialize(IReader reader)
        {
            this.ComponentID = reader.ReadInt32();
            this.EulerAngles = reader.ReadVector3();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
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
                .WriteInt32(ComponentID)
                .WriteVector3(EulerAngles)
                .Done();
        }

        public static RotateComponentPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<RotateComponentPacket>();
            packet.ComponentID = reader.ReadInt32();
            packet.EulerAngles = reader.ReadVector3();

            return packet;
        }
    }
}

using SavedObjects;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Client.Packeting.Packets
{
    public class PlaceComponentPacket : Packet
    {
        public override PacketType Type => PacketType.PlaceComponent;
        public override bool ShouldBroadcast => true;

        public int NetID { get; set; }
        public SavedObjectV2 SavedObject { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 EulerAngles { get; set; }
        public int ParentBoardID { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(NetID)
                .WriteBinaryObject(SavedObject)
                .WriteInt32(ParentBoardID)
                .WriteVector3(LocalPosition)
                .WriteVector3(EulerAngles)
                .Done();
        }

        public static PlaceComponentPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlaceComponentPacket>();
            packet.NetID = reader.ReadInt32();
            packet.SavedObject = reader.ReadBinaryObject<SavedObjectV2>();
            packet.ParentBoardID = reader.ReadInt32();
            packet.LocalPosition = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();

            return packet;
        }
    }
}

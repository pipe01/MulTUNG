using SavedObjects;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
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

        public static PlaceComponentPacket BuildFromLocalComponent(GameObject component)
        {
            var netObj = component.GetComponent<NetObject>();

            if (netObj == null)
            {
                netObj = component.AddComponent<NetObject>();
                netObj.NetID = Random.Range(int.MinValue, int.MaxValue);
            }

            var objInfo = component.GetComponent<ObjectInfo>();

            return new PlaceComponentPacket
            {
                NetID = netObj.NetID,
                SavedObject = SavedObjectUtilities.CreateSavedObjectFrom(objInfo),
                LocalPosition = component.transform.localPosition,
                EulerAngles = component.transform.localEulerAngles,
                ParentBoardID = component.transform.parent?.gameObject.GetComponent<NetObject>()?.NetID ?? 0
            };
        }

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

using PiTung.Console;
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

            int parentId = component.transform.parent?.gameObject.GetComponent<NetObject>()?.NetID ?? 0;

            return new PlaceComponentPacket
            {
                NetID = netObj.NetID,
                SavedObject = SavedObjectUtilities.CreateSavedObjectFrom(objInfo),
                LocalPosition = component.transform.localPosition,
                EulerAngles = component.transform.localEulerAngles,
                ParentBoardID = parentId
            };
        }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(NetID)
                .Write(ParentBoardID)
                .Write(LocalPosition)
                .Write(EulerAngles)
                .WriteBinaryObject(SavedObject)
                .Done();
        }

        public static PlaceComponentPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlaceComponentPacket>();
            packet.NetID = reader.ReadInt32();
            packet.ParentBoardID = reader.ReadInt32();
            packet.LocalPosition = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();
            packet.SavedObject = reader.ReadBinaryObject<SavedObjectV2>();

            return packet;
        }

        public override string ToString() => $"{ParentBoardID}.{NetID}";
    }
}

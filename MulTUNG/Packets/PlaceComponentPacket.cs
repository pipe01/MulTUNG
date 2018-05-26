using SavedObjects;
using UnityEngine;

namespace MulTUNG.Packets
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
                netObj.NetID = NetObject.GetNewID();
                
                if (component.GetComponent<ObjectInfo>()?.ComponentType == ComponentType.Mount)
                {
                    var board = component.GetComponentInChildren<CircuitBoard>();
                    board.gameObject.AddComponent<NetObject>().NetID = NetObject.GetNewID();
                }
            }

            var objInfo = component.GetComponent<ObjectInfo>();

            int parentId = component.transform.parent?.GetComponent<NetObject>()?.NetID ?? 0;

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

        public override void Deserialize(IReader reader)
        {
            this.NetID = reader.ReadInt32();
            this.ParentBoardID = reader.ReadInt32();
            this.LocalPosition = reader.ReadVector3();
            this.EulerAngles = reader.ReadVector3();
            this.SavedObject = reader.ReadBinaryObject<SavedObjectV2>();
        }
    }
}

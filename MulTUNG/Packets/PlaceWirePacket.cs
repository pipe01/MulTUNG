using UnityEngine;

namespace MulTUNG.Packets
{
    public class PlaceWirePacket : Packet
    {
        public override PacketType Type => PacketType.PlaceWire;
        public override bool ShouldBroadcast => true;

        public int NetObj1Id { get; set; }
        public int Point1Id { get; set; }
        public int NetObj2Id { get; set; }
        public int Point2Id { get; set; }
        public int NetID { get; set; }
        
        public static PlaceWirePacket BuildFromLocalWire(Wire wire)
        {
            var wireBeingPlaced = wire.gameObject;
            
            var netObj = wireBeingPlaced.AddComponent<NetObject>();
            netObj.NetID = NetObject.GetNewID();
            
            var netObj1 = GetNetObjectFromPoint(wire.Point1);
            var netObj2 = GetNetObjectFromPoint(wire.Point2);

            if (netObj1 == null || netObj2 == null)
                return null;

            int ioIndex1 = netObj1.IO.IndexOf(wire.Point1.parent.gameObject);
            int ioIndex2 = netObj2.IO.IndexOf(wire.Point2.parent.gameObject);

            if (ioIndex1 == -1 || ioIndex2 == -1)
                return null;

            return new PlaceWirePacket
            {
                NetObj1Id = netObj1.NetID,
                NetObj2Id = netObj2.NetID,
                Point1Id = ioIndex1,
                Point2Id = ioIndex2,
                NetID = netObj.NetID
            };
        }

        private static NetObject GetNetObjectFromPoint(Transform point)
        {
            if (point.parent?.GetComponent<ObjectInfo>() != null)
                return point.parent.GetComponent<NetObject>();

            return point.parent?.parent?.gameObject?.GetComponent<NetObject>();
        }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(NetObj1Id)
                .Write(Point1Id)
                .Write(NetObj2Id)
                .Write(Point2Id)
                .Write(NetID)
                .Done();
        }

        public static PlaceWirePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlaceWirePacket>();
            packet.NetObj1Id = reader.ReadInt32();
            packet.Point1Id = reader.ReadInt32();
            packet.NetObj2Id = reader.ReadInt32();
            packet.Point2Id = reader.ReadInt32();
            packet.NetID = reader.ReadInt32();

            return packet;
        }
    }
}

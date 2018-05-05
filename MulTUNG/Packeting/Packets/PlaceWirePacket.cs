using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
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
            netObj.NetID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            var netObj1 = wire.Point1.parent.parent.gameObject.GetComponent<NetObject>();
            var netObj2 = wire.Point2.parent.parent.gameObject.GetComponent<NetObject>();

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

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .WriteInt32(NetObj1Id)
                .WriteInt32(Point1Id)
                .WriteInt32(NetObj2Id)
                .WriteInt32(Point2Id)
                .WriteInt32(NetID)
                .Done();
        }

        public static PlaceWirePacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

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

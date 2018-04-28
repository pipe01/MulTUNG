using SavedObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Common.Packets
{
    public class PlaceComponentPacket : Packet
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        public override PacketType Type => PacketType.PlaceComponent;

        public SavedObjectV2 SavedObject { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 EulerAngles { get; set; }
        public int ParentBoardID { get; set; }

        protected override byte[] SerializeInner()
        {
            byte[] objData;

            using (MemoryStream mem = new MemoryStream())
            {
                BinFormatter.Serialize(mem, SavedObject);

                objData = mem.ToArray();
            }

            return new PacketBuilder()
                .WriteByteArray(objData)
                .WriteInt32(ParentBoardID)
                .WriteVector3(LocalPosition)
                .WriteVector3(EulerAngles)
                .Done();
        }

        public static PlaceComponentPacket Deserialize(byte[] data)
        {
            var reader = new PacketReader(data);

            var packet = reader.ReadBasePacket<PlaceComponentPacket>();

            using (MemoryStream mem = new MemoryStream(reader.ReadByteArray()))
            {
                packet.SavedObject = (SavedObjectV2)BinFormatter.Deserialize(mem);
            }

            packet.ParentBoardID = reader.ReadInt32();
            packet.LocalPosition = reader.ReadVector3();
            packet.EulerAngles = reader.ReadVector3();

            return packet;
        }
    }
}

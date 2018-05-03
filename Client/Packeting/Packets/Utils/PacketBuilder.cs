using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Client.Packeting.Packets
{
    public class PacketBuilder
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        private MemoryStream Data = new MemoryStream();

        public byte[] Done()
        {
            var data = Data.ToArray();
            Data.Dispose();

            return data;
        }

        private PacketBuilder Write(byte data)
        {
            Data.Write(new byte[] { data }, 0, 1);

            return this;
        }

        public PacketBuilder Write(byte[] data)
        {
            Data.Write(data, 0, data.Length);

            return this;
        }

        public PacketBuilder WritePacketType(PacketType d) => Write((byte)d);

        public PacketBuilder WriteInt32(int d) => Write(BitConverter.GetBytes(d));

        public PacketBuilder WriteInt64(long d) => Write(BitConverter.GetBytes(d));

        public PacketBuilder WriteFloat(float d) => Write(BitConverter.GetBytes(d));

        public PacketBuilder WriteBool(bool d) => Write(BitConverter.GetBytes(d));

        public PacketBuilder WriteVector3(Vector3 d) =>
                 WriteFloat(d.x)
                .WriteFloat(d.y)
                .WriteFloat(d.z);

        public PacketBuilder WriteString(string d)
        {
            WriteInt32(d.Length);
            Write(Encoding.UTF8.GetBytes(d));

            return this;
        }

        public PacketBuilder WriteByteArray(byte[] d)
        {
            WriteInt32(d.Length);
            Write(d);

            return this;
        }

        public PacketBuilder WriteBinaryObject(object obj)
        {
            BinFormatter.Serialize(Data, obj);

            return this;
        }
    }
}

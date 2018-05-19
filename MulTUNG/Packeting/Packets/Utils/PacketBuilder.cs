using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
{
    public class PacketBuilder
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        private MemoryStream Data = new MemoryStream();

        public Stream RawStream => Data;

        public PacketBuilder()
        {
            var data = new byte[sizeof(int)];
        }

        public byte[] Done()
        {
            var data = Data.ToArray();
            Data.Dispose();
            
            return data;
        }
        
        public PacketBuilder WriteRaw(byte[] data)
        {
            Data.Write(data, 0, data.Length);

            return this;
        }

        public PacketBuilder Write(PacketType d) => Write((byte)d);

        public PacketBuilder Write(byte data) => WriteRaw(new[] { data });

        public PacketBuilder Write(int d) => WriteRaw(BitConverter.GetBytes(d));

        public PacketBuilder Write(long d) => WriteRaw(BitConverter.GetBytes(d));

        public PacketBuilder Write(float d) => WriteRaw(BitConverter.GetBytes(d));

        public PacketBuilder Write(bool d) => WriteRaw(BitConverter.GetBytes(d));

        public PacketBuilder Write(Vector3 d) =>
                 Write(d.x)
                .Write(d.y)
                .Write(d.z);

        public PacketBuilder Write(string d) => Write(Encoding.UTF8.GetBytes(d));

        public PacketBuilder Write(byte[] d)
        {
            Write(d.Length);
            WriteRaw(d);

            return this;
        }

        public PacketBuilder WriteBinaryObject(object obj)
        {
            BinFormatter.Serialize(Data, obj);

            return this;
        }
    }
}

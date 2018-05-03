using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Common.Packets
{
    public class PacketReader
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();
        
        private readonly MemoryStream Data;

        public PacketReader(byte[] data)
        {
            this.Data = new MemoryStream(data);
        }

        private byte[] Read(int count)
        {
            byte[] ret = new byte[count];

            this.Data.Read(ret, 0, count);

            return ret;
        }

        public T ReadBasePacket<T>() where T : Packet, new()
        {
            Data.Position = 0;
            ReadPacketType();

            return new T
            {
                Time = ReadFloat(),
                SenderID = ReadInt32()
            };
        }

        public PacketType ReadPacketType() => (PacketType)Read(1)[0];

        public int ReadInt32() => BitConverter.ToInt32(Read(sizeof(int)), 0);

        public long ReadInt64() => BitConverter.ToInt64(Read(sizeof(long)), 0);

        public float ReadFloat() => BitConverter.ToSingle(Read(sizeof(float)), 0);

        public bool ReadBool() => BitConverter.ToBoolean(Read(sizeof(bool)), 0);

        public Vector3 ReadVector3() => new Vector3(ReadFloat(), ReadFloat(), ReadFloat());

        public string ReadString()
        {
            int length = ReadInt32();
            byte[] bytes = Read(length);

            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] ReadByteArray() => Read(ReadInt32());

        public T ReadBinaryObject<T>()
        {
            return (T)BinFormatter.Deserialize(Data);
        }
    }
}

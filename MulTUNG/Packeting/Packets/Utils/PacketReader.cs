using Lidgren.Network;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
{
    public sealed class MessagePacketReader : IReader
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();

        private NetIncomingMessage Message;

        public MessagePacketReader(NetIncomingMessage msg)
        {
            this.Message = msg;
        }

        public T ReadBasePacket<T>() where T : Packet, new()
        {
            Message.Position = 0;

            ReadPacketType();

            return new T
            {
                Time = ReadFloat(),
                SenderID = ReadInt32()
            };
        }

        public T ReadBinaryObject<T>()
        {
            using (MemoryStream mem = new MemoryStream(Message.PeekBytes(Message.LengthBytes - Message.PositionInBytes)))
            {
                return (T)BinFormatter.Deserialize(mem);
            }
        }

        public bool ReadBool() => Message.ReadBoolean();

        public byte[] ReadByteArray() => Message.ReadBytes(Message.ReadInt32());

        public float ReadFloat() => Message.ReadFloat();

        public int ReadInt32() => Message.ReadInt32();

        public long ReadInt64() => Message.ReadInt64();

        public PacketType ReadPacketType() => (PacketType)Message.ReadByte();

        public string ReadString() => Message.ReadString();

        public Vector3 ReadVector3() => Message.ReadVector3();

        public byte[] ReadRaw(int length)
        {
            int maxLength = Message.LengthBytes - Message.PositionInBytes;

            return Message.PeekBytes(Math.Min(maxLength, length));
        }
    }

    public sealed class PacketReader : IReader, IDisposable
    {
        private static BinaryFormatter BinFormatter = new BinaryFormatter();
        
        private readonly MemoryStream Data;

        public int PacketSize { get; }

        public PacketReader(byte[] data) : this(data, 0, data.Length)
        {
        }

        public PacketReader(byte[] data, int offset, int length)
        {
            this.Data = new MemoryStream(data, offset, length);
        }
        

        public void Dispose()
        {
            this.Data.Dispose();
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

        public byte[] ReadRaw(int length) => Read(length);
    }

    public interface IReader
    {
        T ReadBasePacket<T>() where T : Packet, new();
        PacketType ReadPacketType();
        int ReadInt32();
        long ReadInt64();
        float ReadFloat();
        bool ReadBool();
        Vector3 ReadVector3();
        string ReadString();
        byte[] ReadByteArray();
        T ReadBinaryObject<T>();
        byte[] ReadRaw(int length);
    }
}

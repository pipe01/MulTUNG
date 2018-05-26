using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace MulTUNG.Packets
{
    public static class PacketDeserializer
    {
        private static IDictionary<PacketType, ObjectActivator> Builders = new Dictionary<PacketType, ObjectActivator>();

        private delegate object ObjectActivator();

        static PacketDeserializer()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            string @namespace = typeof(Packet).Namespace + ".";

            foreach (PacketType item in Enum.GetValues(typeof(PacketType)))
            {
                var cls = ass.GetType(@namespace + item.ToString() + "Packet");

                if (cls == null)
                    continue;

                var newExp = Expression.New(cls);
                var lambda = Expression.Lambda(typeof(ObjectActivator), newExp);
                var compiled = (ObjectActivator)lambda.Compile();

                Builders.Add(item, compiled);
            }
        }

        public static Packet DeserializePacket(IReader reader)
        {
            var packetTypeEnum = reader.ReadPacketType();

            if (Builders.TryGetValue(packetTypeEnum, out var builder))
            {
                var packet = (Packet)builder();
                packet.Time = reader.ReadFloat();
                packet.SenderID = reader.ReadInt32();
                packet.Deserialize(reader);

                return packet;
            }

            return null;
        }
    }
}

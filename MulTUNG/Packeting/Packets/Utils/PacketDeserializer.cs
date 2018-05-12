using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MulTUNG.Packeting.Packets
{
    public static class PacketDeserializer
    {
        private static IDictionary<PacketType, Func<IReader, Packet>> Handlers = new Dictionary<PacketType, Func<IReader, Packet>>();

        static PacketDeserializer()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            string @namespace = typeof(Packet).Namespace + ".";

            foreach (PacketType item in Enum.GetValues(typeof(PacketType)))
            {
                var cls = ass.GetType(@namespace + item.ToString() + "Packet");

                if (cls == null)
                    continue;

                var method = cls.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);

                Handlers.Add(item, (Func<IReader, Packet>)Delegate.CreateDelegate(typeof(Func<IReader, Packet>), method));
            }
        }

        public static Packet DeserializePacket(IReader reader)
        {
            var packetType = reader.ReadPacketType();

            if (!Handlers.TryGetValue(packetType, out Func<IReader, Packet> handler))
                return null;

            return handler(reader);
        }
    }
}

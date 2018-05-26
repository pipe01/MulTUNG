using System;
using System.Collections.Generic;
using System.Reflection;

namespace MulTUNG.Packets
{
    public static class PacketDeserializer
    {
        private static IDictionary<PacketType, Func<byte[], Packet>> RawHandlers = new Dictionary<PacketType, Func<byte[], Packet>>();

        private static IDictionary<PacketType, Func<IReader, Packet>> Handlers = new Dictionary<PacketType, Func<IReader, Packet>>();

        static PacketDeserializer()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            string @namespace = typeof(Packet).Namespace + ".";

            foreach (PacketType item in Enum.GetValues(typeof(PacketType)))
            {
                var cls = ass.GetType(@namespace + item.ToString() + "Packet");
                var method = cls?.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);

                if (cls == null || method == null || method.GetParameters()[0].ParameterType != typeof(IReader))
                    continue;
                
                Handlers.Add(item, (Func<IReader, Packet>)Delegate.CreateDelegate(typeof(Func<IReader, Packet>), method));
            }
        }

        public static Packet DeserializePacket(IReader reader)
        {
            var packetType = reader.ReadPacketType();

            if (Handlers.TryGetValue(packetType, out var handler))
            {
                return handler(reader);
            }
            else if (RawHandlers.TryGetValue(packetType, out var rawHandler))
            {
                byte[] data = reader.ReadRaw(int.MaxValue);

                return rawHandler(data);
            }

            return null;
        }
    }
}

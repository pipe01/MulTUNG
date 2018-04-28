using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Packets
{
    public static class PacketDeserializer
    {
        private static IDictionary<PacketType, Func<byte[], Packet>> Handlers = new Dictionary<PacketType, Func<byte[], Packet>>
        {
            [PacketType.PlayerWelcome] = PlayerWelcomePacket.Deserialize,
            [PacketType.PlayerState] = PlayerStatePacket.Deserialize,
            [PacketType.PlaceBoard] = PlaceBoardPacket.Deserialize,
            [PacketType.DeleteBoard] = DeleteBoardPacket.Deserialize,
            [PacketType.PlaceComponent] = PlaceComponentPacket.Deserialize,
        };

        public static Packet DeserializePacket(byte[] data)
        {
            return Handlers[(PacketType)data[0]](data);
        }
    }
}

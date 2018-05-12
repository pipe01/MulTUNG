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
        private static IDictionary<PacketType, Func<IReader, Packet>> Handlers = new Dictionary<PacketType, Func<IReader, Packet>>
        {
            [PacketType.Signal]             = SignalPacket.Deserialize,
            [PacketType.PlayerState]        = PlayerStatePacket.Deserialize,
            [PacketType.StateList]          = StateListPacket.Deserialize,
            [PacketType.PlayerWelcome]      = PlayerWelcomePacket.Deserialize,
            [PacketType.PlayerDisconnect]   = PlayerDisconnectPacket.Deserialize,
            [PacketType.PlaceBoard]         = PlaceBoardPacket.Deserialize,
            [PacketType.DeleteBoard]        = DeleteBoardPacket.Deserialize,
            [PacketType.PlaceComponent]     = PlaceComponentPacket.Deserialize,
            [PacketType.DeleteComponent]    = DeleteComponentPacket.Deserialize,
            [PacketType.PlaceWire]          = PlaceWirePacket.Deserialize,
            [PacketType.DeleteWire]         = DeleteWirePacket.Deserialize,
            [PacketType.RotateComponent]    = RotateComponentPacket.Deserialize,
            [PacketType.WorldData]          = WorldDataPacket.Deserialize,
        };

        public static Packet DeserializePacket(IReader reader)
        {
            var packetType = reader.ReadPacketType();

            if (!Handlers.TryGetValue(packetType, out Func<IReader, Packet> handler))
                return null;

            return handler(reader);
        }
    }
}

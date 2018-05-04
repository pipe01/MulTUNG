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
        private static IDictionary<PacketType, Func<byte[], Packet>> Handlers = new Dictionary<PacketType, Func<byte[], Packet>>
        {
            [PacketType.PlayerWelcome] = PlayerWelcomePacket.Deserialize,
            [PacketType.PlayerState] = PlayerStatePacket.Deserialize,
            [PacketType.PlayerDisconnect] = PlayerDisconnectPacket.Deserialize,
            [PacketType.PlaceBoard] = PlaceBoardPacket.Deserialize,
            [PacketType.DeleteBoard] = DeleteBoardPacket.Deserialize,
            [PacketType.PlaceComponent] = PlaceComponentPacket.Deserialize,
            [PacketType.DeleteComponent] = DeleteComponentPacket.Deserialize,
            [PacketType.CircuitUpdate] = _ => new CircuitUpdatePacket(),
            [PacketType.PlaceWire] = PlaceWirePacket.Deserialize,
            [PacketType.DeleteWire] = DeleteWirePacket.Deserialize,
            [PacketType.RotateComponent] = RotateComponentPacket.Deserialize,
        };

        public static Packet DeserializePacket(byte[] data)
        {
            return Handlers[(PacketType)data[0]](data);
        }
    }
}

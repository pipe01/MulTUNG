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
            [PacketType.TransferData]       = TransferDataPacket.Deserialize,
            [PacketType.Signal]             = SignalPacket.Deserialize,
            [PacketType.PlayerState]        = PlayerStatePacket.Deserialize,
            [PacketType.PlayerWelcome]      = PlayerWelcomePacket.Deserialize,
            [PacketType.PlayerDisconnect]   = PlayerDisconnectPacket.Deserialize,
            [PacketType.PlaceBoard]         = PlaceBoardPacket.Deserialize,
            [PacketType.DeleteBoard]        = DeleteBoardPacket.Deserialize,
            [PacketType.PlaceComponent]     = PlaceComponentPacket.Deserialize,
            [PacketType.DeleteComponent]    = DeleteComponentPacket.Deserialize,
            [PacketType.PlaceWire]          = PlaceWirePacket.Deserialize,
            [PacketType.DeleteWire]         = DeleteWirePacket.Deserialize,
            [PacketType.RotateComponent]    = RotateComponentPacket.Deserialize,
        };

        public static Packet DeserializePacket(byte[] data)
        {
            Func<byte[], Packet> handler;

            if (data[0] == 0 || !Handlers.TryGetValue((PacketType)data[0], out handler))
                return null;

            return handler(data);
        }
    }
}

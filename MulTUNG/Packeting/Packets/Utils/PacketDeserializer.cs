﻿using PiTung.Console;
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

        public static Packet DeserializePacket(byte[] data, out int length)
        {
            byte[] packetSizeBytes = new byte[sizeof(int)];
            Array.Copy(data, 0, packetSizeBytes, 0, sizeof(int));

            length = BitConverter.ToInt32(packetSizeBytes, 0) + sizeof(int);

            Func<byte[], Packet> handler;
            byte packetTypeByte = data[sizeof(int)];

            if (packetTypeByte == 0 || !Handlers.TryGetValue((PacketType)packetTypeByte, out handler))
            {
                length = -1;
                return null;
            }

            return handler(data);
        }
    }
}

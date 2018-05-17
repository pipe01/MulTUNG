﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MulTUNG.Packeting.Packets
{
    public class PlayerDataPacket : Packet
    {
        public override PacketType Type => PacketType.PlayerData;

        public string Username { get; set; }

        protected override byte[] SerializeInner()
        {
            return new PacketBuilder()
                .Write(Username)
                .Done();
        }

        public static PlayerDataPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<PlayerDataPacket>();
            packet.Username = reader.ReadString();

            return packet;
        }
    }
}
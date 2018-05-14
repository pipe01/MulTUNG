﻿using System.Collections.Generic;
using UnityEngine;

namespace MulTUNG.Packeting.Packets
{
    public struct PlayerState
    {
        public float Time;
        public int PlayerID;
        public Vector3 Position, EulerAngles;
    }

    public class StateListPacket : Packet
    {
        public override PacketType Type => PacketType.StateList;

        public Dictionary<int, PlayerState> States { get; set; } = new Dictionary<int, PlayerState>();

        protected override byte[] SerializeInner()
        {
            var builder = new PacketBuilder();

            builder.Write(States.Count);

            foreach (var item in States)
            {
                builder.Write(item.Value.PlayerID);
                builder.Write(item.Value.Position);
                builder.Write(item.Value.EulerAngles);
            }

            return builder.Done();
        }

        public static StateListPacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<StateListPacket>();

            int statesCount = reader.ReadInt32();

            for (int i = 0; i < statesCount; i++)
            {
                var state = new PlayerState
                {
                    PlayerID = reader.ReadInt32(),
                    Position = reader.ReadVector3(),
                    EulerAngles = reader.ReadVector3(),
                    Time = packet.Time
                };

                packet.States.Add(state.PlayerID, state);
            }

            return packet;
        }
    }
}

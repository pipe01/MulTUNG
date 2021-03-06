﻿using MulTUNG.Utils;
using PiTung.Console;
using System.Collections.Generic;
using StateKey = System.Collections.Generic.KeyValuePair<int, byte>;

namespace MulTUNG.Packets
{
    public class CircuitStatePacket : Packet
    {
        private static Dictionary<StateKey, bool> CurrentState = new Dictionary<StateKey, bool>();
        private static Dictionary<StateKey, bool> Updated = new Dictionary<StateKey, bool>();

        public override PacketType Type => PacketType.CircuitState;

        public Dictionary<StateKey, bool> States { get; set; }
        public int Count => States?.Count ?? -1;

        protected override byte[] SerializeInner()
        {
            var builder = new PacketBuilder();
            builder.Write(States.Count);

            foreach (var state in States)
            {
                builder.Write(state.Key.Key);
                builder.Write(state.Key.Value);
                builder.Write((byte)(state.Value ? 1 : 0));
            }

            return builder.Done();
        }

        public override void Deserialize(IReader reader)
        {
            int count = reader.ReadInt32();

            this.States = new Dictionary<KeyValuePair<int, byte>, bool>();

            for (int i = 0; i < count; i++)
            {
                int keyKey = reader.ReadInt32();
                byte keyValue = reader.ReadByte();
                bool value = reader.ReadByte() == 1;

                this.States.Add(new KeyValuePair<int, byte>(keyKey, keyValue), value);
            }
        }

        public static CircuitStatePacket Build(bool forceFull = false)
        {
            Dictionary<StateKey, bool> states = null;

            if (forceFull)
            {
                states = CurrentState;
            }
            else
            {
                states = new Dictionary<StateKey, bool>(Updated);
                Updated.Clear();
            }

            return new CircuitStatePacket
            {
                States = states
            };
        }

        public static void SetOutputState(CircuitOutput output, bool value, bool clearCache = false)
        {
            if (ComponentActions.TryGetKeyFromOutput(output, out var key, clearCache))
            {
                CurrentState[key] = Updated[key] = value;
            }
        }
    }
}

using PiTung.Console;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using StateKey = System.Collections.Generic.KeyValuePair<int, byte>;

namespace MulTUNG.Packeting.Packets
{
    public class CircuitStatePacket : Packet
    {
        private static Dictionary<StateKey, bool> CurrentState = new Dictionary<StateKey, bool>();
        private static Dictionary<StateKey, bool> LastState = new Dictionary<StateKey, bool>();

        public override PacketType Type => PacketType.CircuitState;

        public Dictionary<StateKey, bool> States { get; set; }

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

        public static CircuitStatePacket Deserialize(IReader reader)
        {
            var packet = reader.ReadBasePacket<CircuitStatePacket>();
            int count = reader.ReadInt32();

            packet.States = new Dictionary<KeyValuePair<int, byte>, bool>();

            for (int i = 0; i < count; i++)
            {
                int keyKey = reader.ReadInt32();
                byte keyValue = reader.ReadByte();
                bool value = reader.ReadByte() == 1;

                packet.States.Add(new KeyValuePair<int, byte>(keyKey, keyValue), value);
            }

            return packet;
        }

        public static CircuitStatePacket Build(bool forceFull = false)
        {
            Dictionary<StateKey, bool> states;

            if (forceFull)
            {
                IGConsole.Log("Full update " + UnityEngine.Time.time);
                states = CurrentState;
            }
            else
            {
                states = CurrentState.Where(o =>
                {
                    if (LastState.TryGetValue(o.Key, out var last))
                        return last != o.Value;
                    return true;
                }).ToDictionary(o => o.Key, o => o.Value);
            }

            return new CircuitStatePacket
            {
                States = states
            };
        }

        private static void CopyState()
        {
            LastState = new Dictionary<StateKey, bool>(CurrentState);
        }
        
        private static void LoadWorldState()
        {
            CopyState();

            foreach (var obj in NetObject.Alive)
            {
                var objInfo = obj.Value.GetComponent<ObjectInfo>();

                if (objInfo != null && (objInfo.ComponentType == ComponentType.CircuitBoard || objInfo.ComponentType == ComponentType.Mount || objInfo.ComponentType == ComponentType.Wire))
                {
                    continue;
                }

                byte ioCounter = 0;

                foreach (var io in obj.Value.GetComponentsInChildren<CircuitOutput>())
                {
                    var output = io.GetComponent<CircuitOutput>();

                    if (output == null)
                        continue;

                    CurrentState[new KeyValuePair<int, byte>(obj.Key, ioCounter++)] = output.On;
                }
            }
        }
    }
}
